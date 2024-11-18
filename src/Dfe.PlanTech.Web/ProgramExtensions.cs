using System.Diagnostics.CodeAnalysis;
using Contentful.Core;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Caching.Models;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Cookie.Service;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Application.Responses.Commands;
using Dfe.PlanTech.Application.Responses.Queries;
using Dfe.PlanTech.Application.Submissions.Commands;
using Dfe.PlanTech.Application.Submissions.Queries;
using Dfe.PlanTech.Application.Users.Commands;
using Dfe.PlanTech.Application.Users.Helper;
using Dfe.PlanTech.Application.Users.Queries;
using Dfe.PlanTech.Domain.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Cookie;
using Dfe.PlanTech.Domain.Cookie.Interfaces;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Infrastructure.Contentful.Helpers;
using Dfe.PlanTech.Infrastructure.Contentful.Serializers;
using Dfe.PlanTech.Infrastructure.Data;
using Dfe.PlanTech.Infrastructure.Redis;
using Dfe.PlanTech.Web.Authorisation;
using Dfe.PlanTech.Web.Configuration;
using Dfe.PlanTech.Web.Content;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Middleware;
using Dfe.PlanTech.Web.Models.Content.Mapped;
using Dfe.PlanTech.Web.Routing;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;

namespace Dfe.PlanTech.Web;

[ExcludeFromCodeCoverage]
public static class ProgramExtensions
{
    public const string ContentAndSupportServiceKey = "content-and-support";

    public static IServiceCollection AddContentfulServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IContractResolver, DependencyInjectionContractResolver>();

        services.SetupContentfulClient(configuration, "Contentful", HttpClientPolicyExtensions.AddRetryPolicy);

        services.AddScoped((services) =>
        {
            var logger = services.GetRequiredService<ILogger<TextRendererOptions>>();

            return new TextRendererOptions(logger, new List<MarkOption>() {
                new(){
                    Mark = "bold",
                    HtmlTag = "span",
                    Classes = "govuk-body govuk-!-font-weight-bold",
                }});
        });

        services.AddScoped((_) => new ParagraphRendererOptions()
        {
            Classes = "govuk-body",
        });

        services.AddScoped((_) => new HyperlinkRendererOptions()
        {
            Classes = "govuk-link",
        });

        services.AddSingleton((_) =>
        {
            var configValue = configuration["CONTENTFUL_GET_ENTITY_INT"] ?? "4";

            if (!int.TryParse(configValue, out int include))
            {
                throw new FormatException($"Could not parse CONTENTFUL_GET_ENTITY_INT environment variable to int. Value: {configValue}");
            }

            return new GetPageFromContentfulOptions()
            {
                Include = include
            };
        });

        services.AddTransient<GetPageQuery>();

        services.AddOptions<ContentfulOptions>()
                .Configure<IConfiguration>((settings, configuration) => configuration.GetSection("Contentful").Bind(settings));

        services.AddOptions<ApiAuthenticationConfiguration>()
                .Configure<IConfiguration>((settings, configuration) => configuration.GetSection("Api:Authentication").Bind(settings));

        services.AddOptions<SigningSecretConfiguration>()
                .Configure<IConfiguration>((settings, configuration) => configuration.GetSection("Contentful").Bind(settings));

        services.AddTransient((services) => services.GetRequiredService<IOptions<ContentfulOptions>>().Value);
        services.AddTransient((services) => services.GetRequiredService<IOptions<ApiAuthenticationConfiguration>>().Value);
        services.AddTransient((services) => services.GetRequiredService<IOptions<SigningSecretConfiguration>>().Value);

        services.AddTransient<IGetSubTopicRecommendationQuery, GetSubTopicRecommendationQuery>();

        services.AddScoped<ComponentViewsFactory>();

        return services;
    }

    public static IServiceCollection AddCaching(this IServiceCollection services)
    {
        services.AddDistributedMemoryCache();

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromSeconds(10);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Name = ".Dfe.PlanTech";
        });

        services.AddHttpContextAccessor();
        services.AddSingleton<ICacheOptions>(new CacheOptions());
        services.AddTransient<ICacher, Cacher>();
        services.AddTransient<IQuestionnaireCacher, QuestionnaireCacher>();
        services.AddTransient<IUser, UserHelper>();

        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        void databaseOptionsAction(DbContextOptionsBuilder options) => options.UseSqlServer(configuration.GetConnectionString("Database"));

        services.AddDbContext<IPlanTechDbContext, PlanTechDbContext>(databaseOptionsAction);
        ConfigureCookies(services, configuration);

        services.AddTransient<ICalculateMaturityCommand, CalculateMaturityCommand>();
        services.AddTransient<ICreateEstablishmentCommand, CreateEstablishmentCommand>();
        services.AddTransient<ICreateUserCommand, CreateUserCommand>();
        services.AddTransient<IGetEntityFromContentfulQuery, GetEntityFromContentfulQuery>();
        services.AddTransient<IGetEstablishmentIdQuery, GetEstablishmentIdQuery>();
        services.AddTransient<IGetLatestResponsesQuery, GetLatestResponsesQuery>();
        services.AddTransient<IGetNavigationQuery, GetNavigationQuery>();
        services.AddTransient<IGetNextUnansweredQuestionQuery, GetNextUnansweredQuestionQuery>();
        services.AddTransient<IGetSectionQuery, GetSectionQuery>();
        services.AddTransient<IGetSubmissionStatusesQuery, GetSubmissionStatusesQuery>();
        services.AddTransient<IGetUserIdQuery, GetUserIdQuery>();
        services.AddTransient<IProcessSubmissionResponsesCommand, ProcessSubmissionResponsesDto>();
        services.AddTransient<IRecordUserSignInCommand, RecordUserSignInCommand>();
        services.AddTransient<ISubmitAnswerCommand, SubmitAnswerCommand>();
        services.AddTransient<IDeleteCurrentSubmissionCommand, DeleteCurrentSubmissionCommand>();

        return services;
    }

    private static void ConfigureCookies(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICookieService, CookieService>();
        services.AddTransient<ICookiesCleaner, CookiesCleaner>((services) =>
        {
            var options = configuration.GetSection("Cookies").Get<CookiesCleanerOptions>();

            return new CookiesCleaner(options ?? new CookiesCleanerOptions() { EssentialCookies = [] });
        });
    }

    public static IServiceCollection AddGoogleTagManager(this IServiceCollection services)
    {
        services.AddOptions<GtmConfiguration>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("GTM").Bind(settings);
                });

        services.AddTransient(services => services.GetRequiredService<IOptions<GtmConfiguration>>().Value);
        services.AddTransient<GtmService>();
        return services;
    }

    public static IServiceCollection AddCspConfiguration(this IServiceCollection services)
    {
        services.AddTransient<CspConfiguration>();
        return services;
    }

    public static IServiceCollection AddAuthorisationServices(this IServiceCollection services)
    {
        services.AddAuthentication();
        services.AddAuthorizationBuilder()
                        .AddDefaultPolicy(PageModelAuthorisationPolicy.PolicyName, policy =>
                        {
                            policy.Requirements.Add(new PageAuthorisationRequirement());
                            policy.Requirements.Add(new UserOrganisationAuthorisationRequirement());
                        })
                        .AddPolicy(SignedRequestAuthorisationPolicy.PolicyName, policy =>
                        {
                            policy.AddRequirements(new SignedRequestAuthorisationRequirement());
                        });
        services.AddSingleton<ApiKeyAuthorisationFilter>();
        services.AddSingleton<IAuthorizationHandler, SignedRequestAuthorisationPolicy>();
        services.AddSingleton<IAuthorizationHandler, PageModelAuthorisationPolicy>();
        services.AddSingleton<IAuthorizationHandler, UserOrganisationAuthorisationHandler>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, UserAuthorisationMiddlewareResultHandler>();

        return services;
    }

    public static IServiceCollection AddRoutingServices(this IServiceCollection services)
    {
        services.AddTransient<ISubmissionStatusProcessor, SubmissionStatusProcessor>();
        services.AddTransient<IGetRecommendationRouter, GetRecommendationRouter>();
        services.AddTransient<IGetQuestionBySlugRouter, GetQuestionBySlugRouter>();
        services.AddTransient<ICheckAnswersRouter, CheckAnswersRouter>();

        services.AddTransient((_) => SectionCompleteStatusChecker.SectionComplete);
        services.AddTransient((_) => SectionNotStartedStatusChecker.SectionNotStarted);
        services.AddTransient((_) => CheckAnswersOrNextQuestionChecker.CheckAnswersOrNextQuestion);

        return services;
    }

    public static IServiceCollection AddContentAndSupportServices(this WebApplicationBuilder builder)
    {
        builder.InitCsDependencyInjection();

        return builder.Services;
    }

    public static IServiceCollection AddExceptionHandlingServices(this IServiceCollection services)
    {
        services.AddTransient<IExceptionHandlerMiddleware, ServiceExceptionHandlerMiddleWare>();
        services.AddTransient<IUserJourneyMissingContentExceptionHandler, UserJourneyMissingContentExceptionHandler>();

        return services;
    }

    public static IServiceCollection AddCustomTelemetry(this IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetry();
        services.AddSingleton<ITelemetryInitializer, CustomRequestDimensionsTelemetryInitializer>();

        return services;
    }

    public static void InitCsDependencyInjection(this WebApplicationBuilder app)
    {
        app.Services.Configure<TrackingOptions>(app.Configuration.GetSection("tracking"))
            .AddSingleton(sp => sp.GetRequiredService<IOptions<TrackingOptions>>().Value);

        app.Services
            .Configure<SupportedAssetTypes>(app.Configuration.GetSection("cs:supportedAssetTypes"))
            .AddSingleton(sp => sp.GetRequiredService<IOptions<SupportedAssetTypes>>().Value);

        app.Services.SetupContentfulClient(app);

        app.Services.AddKeyedTransient<ICacheService<List<CsPage>>, CsPagesCacheService>(
            ContentAndSupportServiceKey);
        app.Services.AddKeyedTransient<IModelMapper, ModelMapper>(ContentAndSupportServiceKey);
        app.Services
            .AddKeyedTransient<IContentService, ContentService>(ContentAndSupportServiceKey);
        app.Services.AddKeyedTransient<ILayoutService, LayoutService>(ContentAndSupportServiceKey);

        app.Services.Configure<CookiePolicyOptions>(options =>
        {
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.Strict;
            options.ConsentCookieValue = "false";
        });
    }

    public static void SetupContentfulClient(this IServiceCollection services,
        WebApplicationBuilder app)
    {
        app.Services.Configure<Contentful.Core.Configuration.ContentfulOptions>(app.Configuration.GetSection("cs:contentful"))
            .AddKeyedSingleton(ContentAndSupportServiceKey, (IServiceProvider sp) =>
                sp.GetRequiredService<IOptions<Contentful.Core.Configuration.ContentfulOptions>>().Value);

        services.AddKeyedScoped<IContentfulClient, ContentfulClient>(ContentAndSupportServiceKey,
            (sp, _) =>
            {
                var contentfulOptions =
                    sp.GetRequiredKeyedService<Func<IServiceProvider,
                        Contentful.Core.Configuration.ContentfulOptions>>(
                        ContentAndSupportServiceKey)(sp);
                var httpClient = sp.GetRequiredService<HttpClient>();
                return new ContentfulClient(httpClient, contentfulOptions);
            });

        if (app.Environment.EnvironmentName.Equals("e2e"))
            services.AddKeyedScoped<IContentfulService, StubContentfulService>(
                ContentAndSupportServiceKey);
        else
            services.AddKeyedScoped<IContentfulService, ContentfulService>(
                ContentAndSupportServiceKey);

        HttpClientPolicyExtensions.AddRetryPolicy(services.AddHttpClient<ContentfulClient>());
    }
    public static IServiceCollection AddRedisServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(
            new DistributedCachingOptions(ConnectionString: configuration.GetConnectionString("redis") ?? ""));
        services.AddSingleton<ICmsCache, RedisCache>();
        services.AddSingleton<IRedisConnectionManager, RedisConnectionManager>();
        services.AddSingleton<IDistributedLockProvider, RedisLockProvider>();

        return services;
    }
}
