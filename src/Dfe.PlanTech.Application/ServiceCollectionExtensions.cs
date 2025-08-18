
using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Application.Rendering;
using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows;
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

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services
            .AddScoped<ContentfulService>()
            .AddScoped<EstablishmentService>()
            .AddScoped<GroupService>()
            .AddScoped<QuestionService>()
            .AddScoped<RecommendationService>()
            .AddScoped<SubmissionService>();
    }

    public static IServiceCollection AddApplicationWorkflows(this IServiceCollection services)
    {
        return services
            .AddScoped<ContentfulWorkflow>()
            .AddScoped<EstablishmentWorkflow>()
            .AddScoped<RecommendationWorkflow>()
            .AddScoped<SignInWorkflow>()
            .AddScoped<SubmissionWorkflow>()
            .AddScoped<UserWorkflow>();
    }

    private static Func<Type, bool> IsContentRenderer(Type contentRendererType) => (type) =>
        type.IsClass && !type.IsAbstract && type.IsSubclassOf(contentRendererType);
}
