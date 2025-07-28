
using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Application.Rendering;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Application;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection SetupRichTextRenderers(this IServiceCollection services)
    {
        var contentRendererType = typeof(BaseRichTextContentPartRenderer);

        services.AddScoped<IRichTextRenderer, RichTextRenderer>();
        services.AddScoped<IRichTextContentPartRendererCollection, RichTextRenderer>();
        services.AddScoped<ICardContainerContentPartRenderer, GridContainerRenderer>();
        services.AddScoped<ICardContentPartRenderer, CardComponentRenderer>();

        var richTextPartRenderers = contentRendererType.Assembly.GetTypes().Where(IsContentRenderer(contentRendererType));
        foreach (var partRenderer in richTextPartRenderers)
        {
            services.AddScoped(typeof(IRichTextContentPartRenderer), partRenderer);
        }

        return services;
    }

    private static Func<Type, bool> IsContentRenderer(Type contentRendererType) => (type) =>
        type.IsClass && !type.IsAbstract && type.IsSubclassOf(contentRendererType);
}
