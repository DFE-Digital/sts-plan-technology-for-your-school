
using Contentful.Core;
using Contentful.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Dfe.PlanTech.Infrastructure.Contentful.Persistence;
using Dfe.PlanTech.Application.Persistence.Interfaces;

namespace Dfe.PlanTech.Infrastructure.Contentful.Helpers;

public static class ContentfulSetup
{
    
    /// <summary>
    /// Sets up the necessary services for Contentful.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="section"></param>
    /// <see cref="IContentfulClient"/>
    /// <see cref="ContentfulClient"/>
    public static void SetupContentfulClient(this IServiceCollection services, IConfiguration configuration, string section)
    {
        var options = configuration.GetSection(section).Get<ContentfulOptions>() ?? throw new KeyNotFoundException(nameof(ContentfulOptions));
        
        services.AddSingleton(options);
        services.AddHttpClient<ContentfulClient>();
        services.AddScoped<IContentfulClient, ContentfulClient>();
        services.AddScoped<IContentRepository, ContentfulRepository>();
    }
}
