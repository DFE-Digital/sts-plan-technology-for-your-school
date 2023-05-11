using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Application.Helpers;

/// <summary>
/// Adds CQRS classes to application
/// </summary>
/// <param name="services"></param>
/// <returns></returns>

public static class CQRSServicesSetup
{
    public static IServiceCollection AddCQRSServices(this IServiceCollection services)
    {
        var queries = Assembly.GetExecutingAssembly().GetTypes().Where(IsConcreteQueryClass);

        foreach (var query in queries)
        {
            services.AddScoped(query, query);
        }

        return services;
    }

    /// <summary>
    /// Checks if the given type is concrete (not abstract + not interface), and inherits <see href="IInfrastructureQuery"/>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static bool IsConcreteQueryClass(Type type)
    => !type.IsAbstract && !type.IsInterface && type.GetInterface(nameof(IInfrastructureQuery)) != null;

}
