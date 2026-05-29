using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class GroupsViewBuilder(
    ILogger<BaseViewBuilder> logger,
    IOptions<ContactOptionsConfiguration> contactOptions,
    IContentfulService contentfulService,
    ICurrentUser currentUser,
    IEstablishmentService establishmentService,
    IGroupService groupService
) : BaseViewBuilder(logger, contentfulService, currentUser), IGroupsViewBuilder
{
    private readonly IEstablishmentService _establishmentService =
        establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));
    private readonly IGroupService _groupService =
        groupService ?? throw new ArgumentNullException(nameof(groupService));
    private readonly ContactOptionsConfiguration _contactOptions =
        contactOptions?.Value ?? throw new ArgumentNullException(nameof(contactOptions));

    private const string SelectASchoolViewName = "GroupsSelectSchool";
    private const string SelectASelfAssessmentViewName = "GroupsSelectSelfAssessment";
    private const string SelectSchoolsToAssessViewName = "GroupSelectSchoolsToAssess";

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

    public async Task<IActionResult> RouteToSelectASelfAssessmentViewModelAsync(Controller controller)
    {
        // Get the MAT id
        var establishmentId = GetUserOrganisationIdOrThrowException();
        var groupName = CurrentUser.UserOrganisationName ?? "Your organisation";

        var categories = (await ContentfulService.GetAllCategoriesAsync() ?? []).ToList();

        if (categories.Count() == 0)
        {
            throw new Exception("No categories found on groups assessment selection page.");
        }

        // establishments for the MAT
        var matEstablishmentLinks = await _establishmentService.GetEstablishmentLinks(establishmentId) ?? [];

        // Get urn's from links (filter for distinct, white spaces).
        var matEstablishmentUrns = matEstablishmentLinks
            .Select(e => e.Urn)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct()
            .ToArray();

        // Retrieve the actual establishments.
        var matEstablishments = await _establishmentService.GetEstablishmentsByReferencesAsync(matEstablishmentUrns) ?? [];

        // Get the ids of the establishments.
        var matEstablishmentIds = matEstablishments
            .Select(e => e.Id)
            .Distinct()
            .ToArray();

        // Get the completed submissions for the MAT.
        var completedSubmissions = matEstablishmentIds.Length != 0
            ? await _groupService.GetGroupCompletedSubmissionsBySections(matEstablishmentIds) ?? []
            : [];

        var completedCountBySectionId = completedSubmissions
            .Where(cs => matEstablishmentIds.Contains(cs.EstablishmentId))
            .GroupBy(cs => cs.SectionId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(cs => cs.EstablishmentId).Distinct().Count());


        var viewModel = new GroupSelectAssessmentViewModel()
        {
            GroupName = groupName,
            Categories = categories.Select(c => new CategorySectionViewModel
            {
                CategoryName = c.Header?.Text ?? string.Empty,
                Sections = (c.Sections ?? []).Select(ccs =>
                {
                    var completedCount = completedCountBySectionId.GetValueOrDefault(ccs.Id);
                    var uncompletedCount = Math.Max(0, matEstablishmentIds.Length - completedCount);
                    return new GroupSelectAssessmentSectionViewModel()
                    {
                        SectionName = ccs.Name,
                        UncompletedGroupSubmissions = uncompletedCount
                    };
                }).ToList()
            }).ToList()
        };

        return controller.View(SelectASelfAssessmentViewName, viewModel);
    }

    public async Task<IActionResult> RouteToSelectSchoolsToAssessViewModelAsync(
        Controller controller,
        string categorySlug,
        string sectionSlug
    )
    {
        var establishmentId = GetUserOrganisationIdOrThrowException();

        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find topic for slug '{UrlConstants.GroupsSelectionPageSlug}'"
            );

        var viewModel = new GroupsSelectSchoolsToAssessViewModel
        {

        };

        return controller.View(SelectSchoolsToAssessViewName, viewModel);
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
