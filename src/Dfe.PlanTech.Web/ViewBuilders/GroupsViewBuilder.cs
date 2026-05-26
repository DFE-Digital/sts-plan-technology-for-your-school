using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class GroupsViewBuilder(
    ILogger<GroupsViewBuilder> logger,
    IOptions<ContactOptionsConfiguration> contactOptions,
    IContentfulService contentfulService,
    ICurrentUser currentUser,
    IEstablishmentService establishmentService,
    ISubmissionService submissionService
) : BaseViewBuilder(logger, contentfulService, currentUser), IGroupsViewBuilder
{
    private readonly IEstablishmentService _establishmentService =
        establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));

    private readonly ISubmissionService _submissionService =
        submissionService ?? throw new ArgumentNullException(nameof(submissionService));

    private readonly ContactOptionsConfiguration _contactOptions =
        contactOptions?.Value ?? throw new ArgumentNullException(nameof(contactOptions));

    private const string SelectASchoolViewName = "GroupsSelectSchool";

    public async Task<IActionResult> RouteToSelectASchoolViewModelAsync(Controller controller)
    {
        // Get the user's organisation ID (the MAT/group), not the active establishment
        // At this point, the user hasn't selected a school yet
        var establishmentId = GetUserOrganisationIdOrThrowException();

        var selectASchoolPageContent =
            await ContentfulService.GetPageBySlugAsync(UrlConstants.GroupsSelectionPageSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find contentful page for slug '{UrlConstants.GroupsSelectionPageSlug}'"
            );

        var groupName = CurrentUser.UserOrganisationName ?? "Your organisation";
        List<ContentfulEntry> content = selectASchoolPageContent.Content ?? [];

        var sections = await ContentfulService.GetAllSectionsAsync();
        var allRecommendations = sections.SelectMany(section => section.CoreRecommendations);
        string totalRecommendations = allRecommendations.Count().ToString();

        var groupSchools =
            await _establishmentService.GetEstablishmentLinksWithRecommendationCounts(
                establishmentId
            );

        var contactLink = await ContentfulService.GetLinkByIdAsync(_contactOptions.LinkId);

        var viewModel = new GroupsSelectorViewModel
        {
            GroupName = groupName,
            GroupEstablishments = groupSchools,
            BeforeTitleContent = selectASchoolPageContent.BeforeTitleContent ?? [],
            Title = new ComponentTitleEntry(groupName),
            Content = content,
            TotalRecommendations = totalRecommendations,
            ProgressRetrievalErrorMessage = string.IsNullOrEmpty(totalRecommendations)
                ? "Unable to retrieve progress"
                : null,
            ContactLinkHref = contactLink?.Href,
        };

        controller.ViewData[StatePassingMechanismConstants.Title] = "Select a school";
        return controller.View(SelectASchoolViewName, viewModel);
    }

    public async Task<IActionResult> RouteToViewInProgressAnswers(
        Controller controller,
        string categorySlug,
        string sectionSlug
    )
    {
        var establishmentId = await GetActiveEstablishmentIdOrThrowException();

        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find section for slug {sectionSlug}"
            );

        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(
            establishmentId,
            section,
            status: SubmissionStatus.InProgress
        );

        switch (submissionRoutingData.Status)
        {
            case SubmissionStatus.NotStarted:
                return controller.RedirectToHomePage();

            case SubmissionStatus.InProgress:
            case SubmissionStatus.CompleteNotReviewed:

                var viewModel = ReviewAnswersViewBuilder.BuildViewAnswersViewModel(
                section,
                submissionRoutingData,
                categorySlug,
                sectionSlug,
                isMatInProgressView: true,
                schoolName: CurrentUser.GroupSelectedSchoolName
                );

                viewModel.IsMatInProgressView = true;
                viewModel.BackLinkHref = $"/{categorySlug}";

                return controller.View(
                    ReviewAnswersViewBuilder.ViewAnswersViewName,
                    viewModel
                );

            default:
                return controller.RedirectToHomePage();
        }
    }

    public async Task RecordGroupSelectionAsync(
        string selectedEstablishmentUrn,
        string selectedEstablishmentName
    )
    {
        var userDsiReference = GetDsiReferenceOrThrowException();
        var userOrganisationId = CurrentUser.UserOrganisationId;

        // Construct the user's organisation model from individual properties
        var userOrganisationModel = new EstablishmentModel
        {
            Id = CurrentUser.UserOrganisationDsiId ?? Guid.Empty,
            Name = CurrentUser.UserOrganisationName ?? string.Empty,
            Urn = CurrentUser.UserOrganisationUrn,
            Ukprn = CurrentUser.UserOrganisationUkprn,
            Uid = CurrentUser.UserOrganisationUid,
            GroupUid = CurrentUser.UserOrganisationUid, // TODO: resolve some confusion here - the database table is `GroupUid` and is populated from the `uid` OIDC claim - possibly remove `groupUid` from `EstablishmentModel`?
            Type = CurrentUser.UserOrganisationTypeName is null
                ? null
                : new IdWithNameModel { Name = CurrentUser.UserOrganisationTypeName },
        };

        await _establishmentService.RecordGroupSelection(
            userDsiReference,
            userOrganisationId,
            userOrganisationModel,
            selectedEstablishmentUrn,
            selectedEstablishmentName
        );
    }
}
