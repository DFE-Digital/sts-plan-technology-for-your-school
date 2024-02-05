using Dfe.PlanTech.Web.Helpers;

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
        var serviceProvider = services.BuildServiceProvider();

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
            options.UseMemoryCacheProvider().DisableLogging(false).UseCacheKeyPrefix("EF_");
            options.CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30));
            options.UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(1));
        });

        services.AddDbContext<IPlanTechDbContext, PlanTechDbContext>(databaseOptionsAction);

        services.AddTransient<ICalculateMaturityCommand, CalculateMaturityCommand>();
        services.AddTransient<ICookieService, CookieService>();
        services.AddTransient<ICreateEstablishmentCommand, CreateEstablishmentCommand>();
        services.AddTransient<ICreateUserCommand, CreateUserCommand>();
        services.AddTransient<IGetEstablishmentIdQuery, GetEstablishmentIdQuery>();
        services.AddTransient<IGetLatestResponsesQuery, GetLatestResponsesQuery>();
        services.AddTransient<IGetNavigationQuery, GetNavigationQuery>();
        services.AddTransient<IGetNextUnansweredQuestionQuery, GetNextUnansweredQuestionQuery>();
        services.AddTransient<IGetSectionQuery, GetSectionQuery>();
        services.AddTransient<IGetSubmissionStatusesQuery, GetSubmissionStatusesQuery>();
        services.AddTransient<IGetUserIdQuery, GetUserIdQuery>();
        services.AddTransient<IProcessCheckAnswerDtoCommand, ProcessCheckAnswerDtoCommand>();
        services.AddTransient<IRecordUserSignInCommand, RecordUserSignInCommand>();
        services.AddTransient<ISubmitAnswerCommand, SubmitAnswerCommand>();

        services.AddTransient<GetPageFromDbQuery>();
        return services;
    }

    public static IServiceCollection AddGoogleTagManager(this IServiceCollection services)
    {
        services.AddTransient<GtmConfiguration>();
        return services;
    }
}
