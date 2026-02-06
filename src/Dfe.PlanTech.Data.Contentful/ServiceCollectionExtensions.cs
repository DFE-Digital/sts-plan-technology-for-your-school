using System.Diagnostics.CodeAnalysis;
using Contentful.Core;
using Contentful.Core.Configuration;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Data.Contentful.Interfaces;
using Dfe.PlanTech.Data.Contentful.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Data.Contentful;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Sets up the necessary services for Contentful.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="addRetryPolicy">Action to set up Contentful client (add retry policy)</param>
    /// <see cref="IContentfulClient"/>
    /// <see cref="ContentfulClient"/>
    public static IServiceCollection SetupContentfulClient(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IHttpClientBuilder> addRetryPolicy
    )
    {
        var options =
            configuration
                .GetRequiredSection(ConfigurationConstants.Contentful)
                .Get<ContentfulOptions>()
            ?? throw new KeyNotFoundException(nameof(ContentfulOptions));

        services.AddSingleton(options);

        services
            .AddScoped<IContentfulClient, ContentfulClient>()
            .AddKeyedScoped<IContentfulRepository, ContentfulRepository>(
                KeyedServiceConstants.ContentfulRepository
            )
            .AddScoped<IContentfulRepository, CachedContentfulRepository>();

        addRetryPolicy(services.AddHttpClient<ContentfulClient>());

        return services;
    }
}
