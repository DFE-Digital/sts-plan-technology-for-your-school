using Dfe.PlanTech.Application.Providers;
using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Rendering;
using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Application.Rendering.Contentful;
using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Application;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services
            .AddScoped<IContentfulService, ContentfulService>()
            .AddScoped<IEstablishmentService, EstablishmentService>()
            .AddScoped<INotifyService, NotifyService>()
            .AddScoped<IQuestionService, QuestionService>()
            .AddScoped<IRecommendationService, RecommendationService>()
            .AddScoped<ISubmissionService, SubmissionService>()
            .AddScoped<IUserService, UserService>();
    }

    public static IServiceCollection AddApplicationProviders(this IServiceCollection services)
    {
        return services.AddScoped<IMicrocopyProvider, MicrocopyProvider>();
    }

    public static IServiceCollection AddApplicationWorkflows(this IServiceCollection services)
    {
        return services
            .AddScoped<IContentfulWorkflow, ContentfulWorkflow>()
            .AddScoped<IEstablishmentWorkflow, EstablishmentWorkflow>()
            .AddScoped<INotifyWorkflow, NotifyWorkflow>()
            .AddScoped<ISignInWorkflow, SignInWorkflow>()
            .AddScoped<IRecommendationWorkflow, RecommendationWorkflow>()
            .AddScoped<ISubmissionWorkflow, SubmissionWorkflow>()
            .AddScoped<IUserWorkflow, UserWorkflow>();
    }

    public static IServiceCollection SetupRichTextRenderers(this IServiceCollection services)
    {
        var contentRendererType = typeof(BaseRichTextContentPartRenderer);

        services.AddScoped<IRichTextRenderer, RichTextRenderer>();
        services.AddScoped<IRichTextContentPartRendererCollection, RichTextRenderer>();
        services.AddScoped<ICardContainerContentPartRenderer, GridContainerRenderer>();
        services.AddScoped<ICardContentPartRenderer, CardComponentRenderer>();

        var richTextPartRenderers = contentRendererType
            .Assembly.GetTypes()
            .Where(IsContentRenderer(contentRendererType));
        foreach (var partRenderer in richTextPartRenderers)
        {
            services.AddScoped(typeof(IRichTextContentPartRenderer), partRenderer);
        }

        return services;
    }

    private static Func<Type, bool> IsContentRenderer(Type contentRendererType) =>
        (type) => type.IsClass && !type.IsAbstract && type.IsSubclassOf(contentRendererType);
}
