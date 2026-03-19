using System.Security.Authentication;
using System.Text.Json;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class BaseViewBuilder(
    ILogger<BaseViewBuilder> logger,
    IContentfulService contentfulService,
    ICurrentUser currentUser
)
{
    protected readonly ILogger<BaseViewBuilder> Logger = logger;
    protected IContentfulService ContentfulService =
        contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));
    protected ICurrentUser CurrentUser =
        currentUser ?? throw new ArgumentNullException(nameof(currentUser));

    protected const string ShareByEmailViewName = "~/Views/Shared/Email/ShareByEmail.cshtml";

    protected string GetDsiReferenceOrThrowException()
    {
        return CurrentUser.DsiReference
            ?? throw new AuthenticationException("User is not authenticated");
    }

    protected int GetUserIdOrThrowException()
    {
        return CurrentUser.UserId ?? throw new AuthenticationException("User is not authenticated");
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>The PlanTech database ID for the user's organisation (e.g. establishment, or establishment group)</returns>
    /// <exception cref="InvalidDataException"></exception>
    protected int GetUserOrganisationIdOrThrowException()
    {
        return CurrentUser.UserOrganisationId
            ?? throw new InvalidDataException(nameof(CurrentUser.UserOrganisationId));
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>The PlanTech database ID for the selected establishment (e.g. an establishment that a MAT user has selected)</returns>
    /// <exception cref="InvalidDataException"></exception>
    protected async Task<int> GetActiveEstablishmentIdOrThrowException()
    {
        return await CurrentUser.GetActiveEstablishmentIdAsync()
            ?? throw new InvalidDataException(nameof(CurrentUser.GetActiveEstablishmentIdAsync));
    }

    protected static ShareByEmailViewModel BuildShareByEmailViewModel(
        string controllerName,
        string actionName,
        QuestionnaireCategoryEntry category,
        RecommendationChunkEntry? chunk,
        string categorySlug,
        string? sectionSlug,
        string? chunkSlug,
        ShareByEmailInputViewModel? inputModel
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
