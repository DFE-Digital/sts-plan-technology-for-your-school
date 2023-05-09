using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Sts.PlanTech.Application.Questionnaire.Queries;

namespace Sts.PlanTech.Application.Questionnaire.Helpers;

    public static class QuestionnaireAppSetup
    {
        /// <summary>
        /// Adds CQRS classes to application
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuestionnaireCommandsAndQueries(this IServiceCollection services){
            services.AddScoped<GetCategories>();
            
            return services;
        }   
    }
