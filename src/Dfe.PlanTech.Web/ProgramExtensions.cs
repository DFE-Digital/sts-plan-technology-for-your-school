using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Caching.Models;
using Dfe.PlanTech.Application.Cookie.Interfaces;
using Dfe.PlanTech.Application.Cookie.Service;
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
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web;

[ExcludeFromCodeCoverage]
public static class ProgramExtensions
{
    public static IServiceCollection AddContentfulServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<CategoryContractResolver>();
        
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
        services.AddDbContext<IPlanTechDbContext, PlanTechDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("Database")));

        services.AddTransient<IGetUserIdQuery, GetUserIdQuery>();
        services.AddTransient<ICreateUserCommand, CreateUserCommand>();
        services.AddTransient<ICreateEstablishmentCommand, CreateEstablishmentCommand>();
        services.AddTransient<IRecordUserSignInCommand, RecordUserSignInCommand>();

        services.AddTransient<ICreateQuestionCommand, CreateQuestionCommand>();
        services.AddTransient<IRecordQuestionCommand, RecordQuestionCommand>();
        services.AddTransient<IGetQuestionQuery, GetQuestionQuery>();
        services.AddTransient<ICookieService, CookieService>();

        services.AddTransient<ICreateAnswerCommand, CreateAnswerCommand>();
        services.AddTransient<IRecordAnswerCommand, RecordAnswerCommand>();
        services.AddTransient<IGetAnswerQuery, GetAnswerQuery>();

        services.AddTransient<ICreateSubmissionCommand, CreateSubmissionCommand>();
        services.AddTransient<IGetSubmissionQuery, GetSubmissionQuery>();

        services.AddTransient<ICreateResponseCommand, CreateResponseCommand>();
        services.AddTransient<IGetResponseQuery, GetResponseQuery>();
        services.AddTransient<IGetLatestResponseListForSubmissionQuery, GetLatestResponseListForSubmissionQuery>();

        services.AddTransient<ICalculateMaturityCommand, CalculateMaturityCommand>();
        services.AddTransient<IGetSubmissionStatusesQuery, GetSubmissionStatusesQuery>();
        services.AddTransient<ILogger<Category>, Logger<Category>>();        
        
        services.AddTransient<ProcessCheckAnswerDtoCommand>();

        services.AddTransient<GetSubmitAnswerQueries>();
        services.AddTransient<RecordSubmitAnswerCommands>();
        services.AddTransient<SubmitAnswerCommand>();

        services.AddTransient<IGetEstablishmentIdQuery, GetEstablishmentIdQuery>();

        services.AddTransient<Application.Questionnaire.Queries.GetSectionQuery>();
        services.AddTransient<CategoryHelper>();
        services.AddTransient<ICategory, Category>();
        return services;
    }

    public static IServiceCollection AddGoogleTagManager(this IServiceCollection services, IConfiguration configuration)
    {
        var config = new GtmConfiguration();
        configuration.GetSection("GTM").Bind(config);
        services.AddSingleton((services) => config);

        return services;
    }
}
