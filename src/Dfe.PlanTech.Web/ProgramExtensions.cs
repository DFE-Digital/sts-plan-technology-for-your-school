using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Caching.Models;
using Dfe.PlanTech.Application.Converters;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Response.Commands;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Response.Queries;
using Dfe.PlanTech.Application.Submission.Commands;
using Dfe.PlanTech.Application.Submission.Interface;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Application.Submission.Queries;
using Dfe.PlanTech.Application.Users.Commands;
using Dfe.PlanTech.Application.Users.Helper;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Application.Users.Queries;
using Dfe.PlanTech.Domain.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Dfe.PlanTech.Infrastructure.Contentful.Helpers;
using Dfe.PlanTech.Infrastructure.Data;
using Dfe.PlanTech.Web.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Dfe.PlanTech.Web;

[ExcludeFromCodeCoverage]
public static class ProgramExtensions
{
    private const string CACHE_TABLE_NAME = "MemoryCache";
    private const string CACHE_SCHEMA = "dbo";

    public static IServiceCollection AddContentfulServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.SetupContentfulClient(configuration, "Contentful", HttpClientPolicyExtensions.AddRetryPolicy);

        services.AddScoped((services) =>
        {
            var logger = services.GetRequiredService<ILogger<TextRendererOptions>>();

            return new TextRendererOptions(logger, new List<MarkOption>() {
                new MarkOption(){
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

        return services;
    }

    public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDistributedSqlServerCache(options =>
        {
            options.ConnectionString = configuration.GetConnectionString("Database");
            options.SchemaName = CACHE_SCHEMA;
            options.TableName = CACHE_TABLE_NAME;

            var timeToLiveConfig = configuration["Caching:TimeToLiveInMinutes"];

            if (!string.IsNullOrEmpty(timeToLiveConfig) && int.TryParse(timeToLiveConfig, out int timeToLive))
            {
                options.DefaultSlidingExpiration = TimeSpan.FromMinutes(timeToLive);
            }
        });

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromSeconds(10);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Name = ".Dfe.PlanTech";
        });

        services.AddHttpContextAccessor();
        services.AddTransient<ICacher, Cacher>();
        services.AddTransient<IUrlHistory, UrlHistory>();
        services.AddTransient<IQuestionnaireCacher, QuestionnaireCacher>();
        services.AddTransient<IUser, UserHelper>();

        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IPlanTechDbContext, PlanTechDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("Database")));

        services.AddTransient<IGetUserIdQuery, GetUserIdQuery>();
        services.AddTransient<ICreateUserCommand, CreateUserCommand>();
        services.AddTransient<ICreateEstablishmentCommand, CreateEstablishmentCommand>();
        services.AddTransient<IRecordUserSignInCommand, RecordUserSignInCommand>();

        services.AddTransient<ICreateQuestionCommand, CreateQuestionCommand>();
        services.AddTransient<IRecordQuestionCommand, RecordQuestionCommand>();
        services.AddTransient<IGetQuestionQuery, GetQuestionQuery>();

        services.AddTransient<ICreateAnswerCommand, CreateAnswerCommand>();
        services.AddTransient<IRecordAnswerCommand, RecordAnswerCommand>();
        services.AddTransient<IGetAnswerQuery, GetAnswerQuery>();

        services.AddTransient<ICreateSubmissionCommand, CreateSubmissionCommand>();

        services.AddTransient<ICreateResponseCommand, CreateResponseCommand>();
        services.AddTransient<IGetResponseQuery, GetResponseQuery>();

        services.AddTransient<ICalculateMaturityCommand, CalculateMaturityCommand>();
        services.AddTransient<IGetSubmissionStatusesQuery, GetSubmissionStatusesQuery>();

        return services;
    }

    public static IServiceCollection AddGoogleTagManager(this IServiceCollection services, IConfiguration configuration)
    {
        var config = new GtmConfiguration();
        configuration.GetSection("GTM").Bind(config);
        services.AddSingleton((services) => config);

        return services;
    }

    public static IServiceCollection AddJsonOptions(this IServiceCollection services)
    {
        services.AddScoped(_ =>
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonConverterFactoryForStackOfT());

            return options;
        });

        return services;
    }
}
