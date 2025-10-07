
using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Application.Rendering;
using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Interfaces;
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
            .AddScoped<IContentfulService, ContentfulService>()
            .AddScoped<IEstablishmentService, EstablishmentService>()
            .AddScoped<IQuestionService, QuestionService>()
            .AddScoped<ISubmissionService, SubmissionService>()
            .AddScoped<IRecommendationService, RecommendationService>()
            ;
    }

    public static IServiceCollection AddApplicationWorkflows(this IServiceCollection services)
    {
        return services
            .AddScoped<IContentfulWorkflow, ContentfulWorkflow>()
            .AddScoped<IEstablishmentWorkflow, EstablishmentWorkflow>()
            .AddScoped<ISignInWorkflow, SignInWorkflow>()
            .AddScoped<ISubmissionWorkflow, SubmissionWorkflow>()
            .AddScoped<IUserWorkflow, UserWorkflow>();
    }

    private static Func<Type, bool> IsContentRenderer(Type contentRendererType) => (type) =>
        type.IsClass && !type.IsAbstract && type.IsSubclassOf(contentRendererType);
}
