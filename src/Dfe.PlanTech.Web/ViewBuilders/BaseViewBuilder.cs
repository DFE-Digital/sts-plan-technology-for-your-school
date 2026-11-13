using System.Text.Json;
using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class BaseViewBuilder(
    ILogger<BaseViewBuilder> logger,
    IContentfulService contentfulService,
    ICurrentUserProvider currentUser
)
{
    protected readonly ILogger<BaseViewBuilder> Logger = logger;
    protected IContentfulService ContentfulService =
        contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));
    protected ICurrentUserProvider CurrentUser =
        currentUser ?? throw new ArgumentNullException(nameof(currentUser));

    protected const string ShareByEmailViewName = "~/Views/Shared/Email/ShareByEmail.cshtml";

    protected static ShareByEmailViewModel BuildShareByEmailViewModel(
        string controllerName,
        string actionName,
        QuestionnaireCategoryEntry category,
        RecommendationChunkEntry? chunk,
        string categorySlug,
        string? sectionSlug,
        string? chunkSlug,
        ShareByEmailInputViewModel? inputModel,
        bool isMat = false
    )
    {
        var heading = chunkSlug is null
            ? "Share list of recommendations by email"
            : "Share a recommendation by email";

        return new ShareByEmailViewModel
        {
            PostController = controllerName.GetControllerNameSlug(),
            PostAction = actionName,
            CategorySlug = categorySlug,
            SectionSlug = sectionSlug,
            ChunkSlug = chunkSlug,
            Caption = chunk?.HeaderText ?? category.Header.Text,
            Heading = heading,
            InputModel = inputModel,
            IsMat = isMat,
        };
    }

    protected static RedirectToActionResult HandleNotifyShareResults(
        Controller controller,
        List<NotifySendResult> notifySendResults,
        ActionViewModel actionViewModel
    )
    {
        var shareResultsModel = new NotifyShareResultsViewModel
        {
            SendResults = notifySendResults,
            ActionModel = actionViewModel,
        };

        var resultsJson = JsonSerializer.Serialize(shareResultsModel);
        controller.TempData[StatePassingMechanismConstants.NotifyShareResults] = resultsJson;

        if (notifySendResults.All(r => r.Errors.Count == 0))
        {
            return controller.RedirectToAction(
                actionViewModel.ActionName,
                actionViewModel.ControllerName,
                actionViewModel.RouteValues
            );
        }

        return PageRedirecter.RedirectToNotifyError(controller);
    }
}
