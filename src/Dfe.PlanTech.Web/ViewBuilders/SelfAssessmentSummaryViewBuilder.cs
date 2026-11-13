using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class SelfAssessmentSummaryViewBuilder(
    ILogger<SelfAssessmentSummaryViewBuilder> logger,
    IContentfulService contentfulService,
    ICurrentUserProvider currentUser,
    IGroupService groupService,
    IHttpContextAccessor httpContextAccessor
) : BaseViewBuilder(logger, contentfulService, currentUser), ISelfAssessmentSummaryViewBuilder
{
    private readonly IGroupService _groupService =
        groupService ?? throw new ArgumentNullException(nameof(groupService));

    private readonly IHttpContextAccessor _httpContextAccessor =
        httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    public const string SelfAssessmentSummaryViewName =
        "~/Views/Shared/SelfAssessmentSummary.cshtml";

    public async Task<IActionResult> RouteToSelfAssessmentSummary(
        Controller controller,
        string categorySlug,
        string sectionSlug
    )
    {
        var viewModel = await BuildSelfAssessmentSummaryViewModel(categorySlug, sectionSlug);

        if (CurrentUser.IsMat && viewModel.CompletedSchoolCount == 0)
        {
            return controller.Redirect($"/trust/{UrlConstants.GroupSelfAssessmentSelectionSlug}");
        }

        return controller.View(SelfAssessmentSummaryViewName, viewModel);
    }

    private async Task<SelfAssessmentSummaryViewModel> BuildSelfAssessmentSummaryViewModel(
        string categorySlug,
        string sectionSlug
    )
    {
        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find section for slug {sectionSlug}"
            );

        var sectionId =
            section.Sys?.Id
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find section id for slug {sectionSlug}"
            );

        var categories = await ContentfulService.GetAllCategoriesAsync() ?? [];

        var category = categories.FirstOrDefault(c =>
            c.Sections?.Any(s => s.Id == sectionId) == true
        );

        var categoryName = category?.Header?.Text ?? categorySlug.Replace("-", " ");
        var recommendationCategorySlug = category?.LandingPage?.Slug ?? categorySlug;

        var submittedSubmissions = await GetSubmittedSubmissions(sectionId);

        return new SelfAssessmentSummaryViewModel
        {
            SectionName = section.Name,
            CategoryName = categoryName,
            CompletedSchoolCount = submittedSubmissions.Count,
            IsMatSummary = CurrentUser.IsMat,
            RecommendationLinks = submittedSubmissions.Select(BuildRecommendationLink).ToList(),
            ShowSubmitAnotherSelfAssessment = true,
            SubmitAnotherSelfAssessmentHref = CurrentUser.IsMat
                ? $"/trust/{UrlConstants.GroupSelfAssessmentSelectionSlug}"
                : $"/{recommendationCategorySlug}",
            BackToHomeHref = UrlConstants.HomePage,
        };

        SelfAssessmentSummaryRecommendationLinkViewModel BuildRecommendationLink(
            SqlSubmissionDto submission
        )
        {
            var organisationName = submission.Establishment?.OrgName ?? "Unknown Establishment";

            return new SelfAssessmentSummaryRecommendationLinkViewModel
            {
                LinkText = CurrentUser.IsMat
                    ? organisationName
                    : $"View the recommendations for {section.Name.ToLowerInvariant()}",

                SchoolUrn = CurrentUser.IsMat ? submission.Establishment!.EstablishmentRef : null,

                SchoolName = CurrentUser.IsMat ? submission.Establishment!.OrgName : null,

                Href = CurrentUser.IsMat
                    ? $"/trust/{recommendationCategorySlug}"
                    : $"/{recommendationCategorySlug}",
            };
        }
    }

    private async Task<List<SqlSubmissionDto>> GetSubmittedSubmissions(string sectionId)
    {
        var selectedSchoolIds = CurrentUser.GetSelectedSchoolEstablishmentIds();
        var focusedSchoolId = CurrentUser.GetFocusedSchool().EstablishmentId;

        var establishmentIds = selectedSchoolIds.Union([focusedSchoolId]).ToArray();

        var completedSubmissions = await _groupService.GetGroupCompletedSubmissionsBySections(
            establishmentIds
        );

        return completedSubmissions.Where(s => s.SectionId == sectionId).ToList();
    }
}
