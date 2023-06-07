
using Contentful.Core;
using Contentful.Core.Configuration;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;
using Dfe.PlanTech.Infrastructure.Contentful.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

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

        SetupHttpClient(services);

        services.AddScoped<IContentfulClient, ContentfulClient>();
        services.AddScoped<IContentRepository, ContentfulRepository>();

        services.SetupRichTextRenderer();

        return services;
    }

    public static IServiceCollection SetupRichTextRenderer(this IServiceCollection services)
    {
        var contentRendererType = typeof(BaseRichTextContentPartRender);
        var richTextPartRenderers = contentRendererType.Assembly.GetTypes()
                                                                .Where(IsContentRenderer(contentRendererType));

        services.AddScoped<IRichTextRenderer, RichTextRenderer>();
        services.AddScoped<IRichTextContentPartRendererCollection, RichTextRenderer>();

        foreach (var partRenderer in richTextPartRenderers)
        {
            services.AddScoped(typeof(IRichTextContentPartRenderer), partRenderer);
        }

        return services;
    }

    private static void SetupHttpClient(IServiceCollection services)
    {
        services.AddHttpClient<ContentfulClient>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetRetryPolicy());
    }

    private static Func<Type, bool> IsContentRenderer(Type contentRendererType)
        => type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(contentRendererType);

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        => HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

}
