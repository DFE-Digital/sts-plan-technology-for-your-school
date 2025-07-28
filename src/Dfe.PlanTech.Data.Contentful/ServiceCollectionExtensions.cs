
using System.Diagnostics.CodeAnalysis;
using Contentful.Core;
using Contentful.Core.Configuration;
using Dfe.PlanTech.Data.Contentful.Interfaces;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Repositories;
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
    /// <param name="section"></param>
    /// <param name="setupClient">Action to setup ContentfulClient (e.g. retry policy)</param>
    /// <see cref="IContentfulClient"/>
    /// <see cref="ContentfulClient"/>
    public static IServiceCollection SetupContentfulClient(this IServiceCollection services, IConfiguration configuration, string section, Action<IHttpClientBuilder> setupClient)
    {
        var options = configuration.GetSection(section).Get<ContentfulOptions>() ?? throw new KeyNotFoundException(nameof(ContentfulOptions));

        services.AddSingleton(options);

        services.AddScoped<IContentfulClient, ContentfulClient>();
        services.AddKeyedScoped<IContentfulRepository, ContentfulRepository>("contentfulRepository");
        services.AddScoped<IContentfulRepository, CachedContentfulRepository>();


        setupClient(services.AddHttpClient<ContentfulClient>());

        return services;
    }
}
