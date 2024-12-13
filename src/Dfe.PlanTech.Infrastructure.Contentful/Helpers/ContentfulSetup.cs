
using System.Diagnostics.CodeAnalysis;
using Contentful.Core;
using Contentful.Core.Configuration;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submissions.Queries;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;
using Dfe.PlanTech.Infrastructure.Contentful.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Contentful.Helpers;

[ExcludeFromCodeCoverage]
public static class ContentfulSetup
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

        services.AddTransient<IGetSubmissionStatusesQuery, GetSubmissionStatusesQuery>();        
        services.AddScoped<IContentfulClient, ContentfulClient>();
        services.AddScoped<IContentRepository, ContentfulRepository>();


        services.SetupRichTextRenderer();
        setupClient(services.AddHttpClient<ContentfulClient>());

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

    private static Func<Type, bool> IsContentRenderer(Type contentRendererType)
        => type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(contentRendererType);
}
