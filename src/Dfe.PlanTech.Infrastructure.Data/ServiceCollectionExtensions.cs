
using System.Diagnostics.CodeAnalysis;
using Contentful.Core;
using Contentful.Core.Configuration;
using Dfe.PlanTech.Data.Contentful;
using Dfe.PlanTech.Data.Contentful.PartRenderers;
using Dfe.PlanTech.Data.Contentful.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Data.Contentful.Helpers;

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
        var options = configuration.GetSection(section).GetSection<ContentfulOptions>() ?? throw new KeyNotFoundException(nameof(ContentfulOptions));

        services.AddSingleton(options);

        services.AddKeyedScoped<IContentRepository, ContentfulRepository>("contentfulRepository");
        services.AddScoped<IContentRepository, CachedContentfulRepository>();

        services.SetupRichTextRenderer();
        setupClient(services.AddHttpClient<ContentfulClient>());

        return services;
    }

    public static IServiceCollection SetupRichTextRenderer(this IServiceCollection services)
    {
        var contentRendererType = typeof(BaseRichTextContentPartRender);
        var richTextPartRenderers = contentRendererType.Assembly.GetTypes()
                                                                .Where(IsContentRenderer(contentRendererType));

        services.AddScoped<RichTextRenderer>();
        services.AddScoped<GridContainerRenderer>();
        services.AddScoped<CardComponent>();

        return services;
    }

    private static Func<Type, bool> IsContentRenderer(Type contentRendererType)
        => type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(contentRendererType);
}
