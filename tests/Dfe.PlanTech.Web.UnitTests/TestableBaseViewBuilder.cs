using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Web.UnitTests;

public sealed class TestableBaseViewBuilder(
    ILogger<BaseViewBuilder> logger,
    IContentfulService contentfulService,
    ICurrentUser currentUser
) : BaseViewBuilder(logger, contentfulService, currentUser)
{
    public string CallGetDsiReferenceOrThrowException() => GetDsiReferenceOrThrowException();

    public int CallGetUserIdOrThrowException() => GetUserIdOrThrowException();

    public int CallGetUserOrganisationIdOrThrowException() =>
        GetUserOrganisationIdOrThrowException();

    public Task<int> CallGetActiveEstablishmentIdOrThrowException() =>
        GetActiveEstablishmentIdOrThrowException();

    public static ShareByEmailViewModel CallBuildShareByEmailViewModel(
        string controllerName,
        string actionName,
        QuestionnaireCategoryEntry category,
        RecommendationChunkEntry? chunk,
        string categorySlug,
        string? sectionSlug,
        string? chunkSlug,
        ShareByEmailInputViewModel? inputModel
    ) =>
        BuildShareByEmailViewModel(
            controllerName,
            actionName,
            category,
            chunk,
            categorySlug,
            sectionSlug,
            chunkSlug,
            inputModel
        );

    public static RedirectToActionResult CallHandleNotifyShareResults(
        Controller controller,
        List<NotifySendResult> notifySendResults,
        ActionViewModel returnToModel
    ) => HandleNotifyShareResults(controller, notifySendResults, returnToModel);
}
