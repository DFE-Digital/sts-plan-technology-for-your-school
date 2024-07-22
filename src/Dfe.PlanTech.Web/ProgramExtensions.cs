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
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Infrastructure.Contentful.Helpers;
using Dfe.PlanTech.Infrastructure.Contentful.Serializers;
using Dfe.PlanTech.Infrastructure.Data;
using Dfe.PlanTech.Infrastructure.Data.Repositories;
using Dfe.PlanTech.Web.Helpers;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web;

[ExcludeFromCodeCoverage]
public static class ProgramExtensions
{
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

        services.AddTransient<GetPageFromContentfulQuery>();
        services.AddSingleton(new ContentfulOptions(configuration.GetValue<bool>("Contentful:UsePreview")));
        services.AddSingleton(new CacheRefreshConfiguration(
            configuration["CacheClear:Endpoint"],
            configuration["CacheClear:ApiKeyName"],
            configuration["CacheClear:ApiKeyValue"]
            ));

        services.AddKeyedTransient<IGetSubTopicRecommendationQuery, GetSubtopicRecommendationFromContentfulQuery>(GetSubtopicRecommendationFromContentfulQuery.ServiceKey);
        services.AddKeyedTransient<IGetSubTopicRecommendationQuery, GetSubTopicRecommendationFromDbQuery>(GetSubTopicRecommendationFromDbQuery.ServiceKey);
        services.AddTransient<IGetSubTopicRecommendationQuery, GetSubTopicRecommendationQuery>();
        services.AddTransient<IRecommendationsRepository, RecommendationsRepository>();
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
        services.AddSingleton<ICacheOptions>((services) => new CacheOptions());
        services.AddTransient<ICacher, Cacher>();
        services.AddTransient<IQuestionnaireCacher, QuestionnaireCacher>();
        services.AddTransient<IUser, UserHelper>();

        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        IServiceProvider serviceProvider = services.BuildServiceProvider();

        void databaseOptionsAction(DbContextOptionsBuilder options) => options.UseSqlServer(configuration.GetConnectionString("Database"));

        services.AddDbContextPool<ICmsDbContext, CmsDbContext>((serviceProvider, optionsBuilder) =>
            optionsBuilder
                .UseSqlServer(
                    configuration.GetConnectionString("Database"),
                    sqlServerOptionsBuilder =>
                    {
                        sqlServerOptionsBuilder
                            .CommandTimeout((int)TimeSpan.FromSeconds(30).TotalSeconds)
                            .EnableRetryOnFailure();
                    })
                .AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>())
        );

        services.AddEFSecondLevelCache(options =>
        {
            options.UseMemoryCacheProvider().ConfigureLogging(false).UseCacheKeyPrefix("EF_");
            options.CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30));
            options.UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(1));
        });

        services.AddDbContext<IPlanTechDbContext, PlanTechDbContext>(databaseOptionsAction);
        ConfigureCookies(services, configuration);

        services.AddTransient<ICalculateMaturityCommand, CalculateMaturityCommand>();
        services.AddTransient<ICreateEstablishmentCommand, CreateEstablishmentCommand>();
        services.AddTransient<ICreateUserCommand, CreateUserCommand>();
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

        services.AddTransient<GetPageFromDbQuery>();

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
        services.AddTransient<GtmConfiguration>();
        return services;
    }

    public static IServiceCollection AddCspConfiguration(this IServiceCollection services)
    {
        services.AddTransient<CspConfiguration>();
        return services;
    }
}
