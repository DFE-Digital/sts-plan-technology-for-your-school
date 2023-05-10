
using Contentful.Core;
using Contentful.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Dfe.PlanTech.Infrastructure.Contentful.Persistence;
using Dfe.PlanTech.Infrastructure.Persistence;

namespace Dfe.PlanTech.Infrastructure.Contentful.Helpers;

public static class ContentfulSetup
{

    public static void SetupContentfulClient(this IServiceCollection services, IConfiguration configuration, string section)
    {
        var options = configuration.GetSection(section).Get<ContentfulOptions>();

        if (options == null) throw new KeyNotFoundException(nameof(ContentfulOptions));

        services.AddSingleton<ContentfulOptions>(options);
        services.AddHttpClient<ContentfulClient>();
        services.AddScoped<IContentfulClient, ContentfulClient>();
        services.AddScoped<IContentRepository, ContentfulRepository>();
    }
}
