using Microsoft.Extensions.DependencyInjection;
using Dfe.PlanTech.Application.Questionnaire.Queries;

namespace Dfe.PlanTech.Application.Questionnaire.Helpers;

public static class QuestionnaireAppSetup
    {
        /// <summary>
        /// Adds CQRS classes to application
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuestionnaireCommandsAndQueries(this IServiceCollection services){
            services.AddScoped<GetCategoriesQuery>();
            
            return services;
        }   
    }
