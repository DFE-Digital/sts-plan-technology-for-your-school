using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Application;
using Dfe.PlanTech.Application.Background;
using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Application.Workflows.Options;
using Dfe.PlanTech.Core.Caching;
using Dfe.PlanTech.Core.Caching.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models.Options;
using Dfe.PlanTech.Data.Contentful;
using Dfe.PlanTech.Data.Contentful.Serializers;
using Dfe.PlanTech.Infrastructure.Redis;
using Dfe.PlanTech.Web.Authorisation.Filters;
using Dfe.PlanTech.Web.Authorisation.Handlers;
using Dfe.PlanTech.Web.Authorisation.Policies;
using Dfe.PlanTech.Web.Authorisation.Requirements;
using Dfe.PlanTech.Web.Background;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.Factories;
using Dfe.PlanTech.Web.Handlers;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Middleware;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using GovUk.Frontend.AspNetCore;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;

namespace Dfe.PlanTech.Web;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public const string ContentAndSupportServiceKey = "content-and-support";

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
            }
        );

        return services
            .AddSingleton<ApiKeyAuthorisationFilter>()
            .AddSingleton<IAuthorizationHandler, SignedRequestAuthorisationPolicy>()
            .AddSingleton<IAuthorizationHandler, PageModelAuthorisationPolicy>()
            .AddSingleton<IAuthorizationHandler, UserOrganisationAuthorisationHandler>()
            .AddSingleton<IAuthorizationMiddlewareResultHandler, UserAuthorisationMiddlewareResultHandler>();
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
        services.AddTransient<ICacher, CacheHelper>();
        services.AddTransient<QuestionnaireCacher>();

        return services;
    }

    public static IServiceCollection AddContentfulServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IContractResolver, DependencyInjectionContractResolver>();

        services.SetupContentfulClient(configuration, HttpClientPolicyExtensions.AddRetryPolicy);
        services.SetupRichTextRenderers();

        services.AddScoped((servicesInner) =>
        {
            var logger = servicesInner.GetRequiredService<ILogger<TextRendererOptions>>();

            return new TextRendererOptions(logger, [
                new(){
                    Mark = "bold",
                    HtmlTag = "span",
                    Classes = "govuk-body govuk-!-font-weight-bold",
                }]);
        });

        services.AddSingleton((_) =>
        {
            var configValue = configuration["CONTENTFUL_GET_ENTITY_INT"] ?? "5";

            if (!int.TryParse(configValue, out int include))
            {
                throw new FormatException($"Could not parse CONTENTFUL_GET_ENTITY_INT environment variable to int. Value: {configValue}");
            }

            return new GetPageFromContentfulOptions()
            {
                Include = include
            };
        });

        services.AddScoped((_) => new HyperlinkRendererOptions()
        {
            Classes = "govuk-link",
        });

        services.AddScoped((_) => new RichTextPartRendererOptions());

        services.AddOptions<ApiAuthenticationConfiguration>()
            .Configure<IConfiguration>((settings, configuration) => configuration.GetRequiredSection(ConfigurationConstants.ApiAuthentication).Bind(settings));

        services.AddOptions<ContentfulOptionsConfiguration>()
            .Configure<IConfiguration>((settings, configuration) => configuration.GetRequiredSection(ConfigurationConstants.Contentful).Bind(settings));

        services.AddOptions<SigningSecretConfiguration>()
                .Configure<IConfiguration>((settings, configuration) => configuration.GetRequiredSection(ConfigurationConstants.Contentful).Bind(settings));

        services.AddTransient((services) => services.GetRequiredService<IOptions<ApiAuthenticationConfiguration>>().Value);
        services.AddTransient((services) => services.GetRequiredService<IOptions<ContentfulOptionsConfiguration>>().Value);
        services.AddTransient((services) => services.GetRequiredService<IOptions<SigningSecretConfiguration>>().Value);

        services.AddScoped<ComponentViewsFactory>();

        return services;
    }

    public static IServiceCollection AddCookies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICookieService, CookieService>();
        services.AddTransient<ICookieWorkflow, CookieWorkflow>((services) =>
        {
            var options = configuration.GetRequiredSection(ConfigurationConstants.Cookies).Get<CookieWorkflowOptions>();

            return new CookieWorkflow(options ?? new CookieWorkflowOptions() { EssentialCookies = [] });
        });

        return services;
    }

    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        return services
            .AddScoped<ICurrentUser, CurrentUser>();
    }

    public static IServiceCollection AddCustomTelemetry(this IServiceCollection services)
    {
        return services
            .AddApplicationInsightsTelemetry()
            .AddSingleton<ITelemetryInitializer, CustomRequestDimensionsTelemetryInitializer>();
    }

    public static IServiceCollection AddExceptionHandlingServices(this IServiceCollection services)
    {
        services.AddTransient<IExceptionHandlerMiddleware, ServiceExceptionHandlerMiddleware>();
        services.AddTransient<IUserJourneyMissingContentExceptionHandler, UserJourneyMissingContentExceptionHandler>();

        return services;
    }

    public static IServiceCollection AddGoogleTagManager(this IServiceCollection services)
    {
        services.AddOptions<GoogleTagManagerConfiguration>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetRequiredSection(ConfigurationConstants.GoogleTagManager).Bind(settings);
                });

        services.AddTransient(services => services.GetRequiredService<IOptions<GoogleTagManagerConfiguration>>().Value);
        services.AddTransient<GoogleTagManagerServiceServiceConfiguration>();
        return services;
    }

    public static IServiceCollection AddGovUkFrontendConfiguration(this IServiceCollection services)
    {
        services.AddOptions<GovUkFrontendOptions>()
           .Configure<IConfiguration>((settings, configuration) => configuration.GetRequiredSection(ConfigurationConstants.GovUkFrontend).Bind(settings));

        services.AddTransient((services) =>
        {
            var x = services.GetRequiredService<IOptions<GovUkFrontendOptions>>().Value;
            return x;
        });

        return services;
    }

    public static IServiceCollection AddRedisServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(
            new DistributedCachingOptions(ConnectionString: configuration.GetConnectionString("redis") ?? ""));
        services.AddSingleton<ICmsCache, RedisCache>();
        services.AddSingleton<IRedisConnectionManager, RedisConnectionManager>();
        services.AddSingleton<IDistributedLockProvider, RedisLockProvider>();

        services.AddSingleton<IRedisDependencyManager, RedisDependencyManager>();

        services.AddOptions<BackgroundTaskQueueOptions>()
                .Configure<IConfiguration>((settings, configuration) => configuration.GetSection(ConfigurationConstants.BackgroundTaskQueue).Bind(settings));
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<BackgroundTaskHostedService>();

        return services;
    }

    public static IServiceCollection AddRoutingServices(this IServiceCollection services)
    {
        services.AddTransient<ICategorySectionViewComponentViewBuilder, CategorySectionViewComponentViewBuilder>();
        services.AddTransient<ICmsViewBuilder, CmsViewBuilder>();
        services.AddTransient<IFooterLinksViewComponentViewBuilder, FooterLinksViewComponentViewBuilder>();
        services.AddTransient<IGroupsViewBuilder, GroupsViewBuilder>();
        services.AddTransient<IPagesViewBuilder, PagesViewBuilder>();
        services.AddTransient<IQuestionsViewBuilder, QuestionsViewBuilder>();
        services.AddTransient<IRecommendationsViewBuilder, RecommendationsViewBuilder>();
        services.AddTransient<IReviewAnswersViewBuilder, ReviewAnswersViewBuilder>();

        return services;
    }

    public static IServiceCollection AddViewComponents(this IServiceCollection services)
    {
        services.AddTransient<ICategoryLandingViewComponentViewBuilder, CategoryLandingViewComponentViewBuilder>();

        return services;
    }
}
