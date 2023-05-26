
using Contentful.Core;
using Contentful.Core.Configuration;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Infrastructure.Contentful.Content;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers;
using Dfe.PlanTech.Infrastructure.Contentful.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
    public static IServiceCollection SetupContentfulClient(this IServiceCollection services, IConfiguration configuration, string section)
    {
        var options = configuration.GetSection(section).Get<ContentfulOptions>() ?? throw new KeyNotFoundException(nameof(ContentfulOptions));

        services.AddSingleton(options);
        services.AddHttpClient<ContentfulClient>();
        services.AddScoped<IContentfulClient, ContentfulClient>();
        services.AddScoped<IContentRepository, ContentfulRepository>();

        services.SetupRichTextRenderer();

        return services;
    }

    public static IServiceCollection SetupRichTextRenderer(this IServiceCollection services)
    {
        var richTextPartRenderers = typeof(RichTextContentRender).Assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(RichTextContentRender)));

        services.AddScoped<IRichTextRenderer, RichTextRenderer>();
        services.AddScoped<IRichTextContentPartRendererCollection, RichTextRenderer>();

        foreach (var partRenderer in richTextPartRenderers)
        {
            services.AddScoped(typeof(IRichTextContentPartRenderer), partRenderer);
        }

        return services;
    }
}
