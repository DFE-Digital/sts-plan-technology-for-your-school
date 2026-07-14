using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Http;
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
        var viewModel = await BuildSelfAssessmentSummaryViewModel(
            categorySlug,
            sectionSlug
        );

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

        var sectionId = section.Sys?.Id
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find section id for slug {sectionSlug}"
            );

        var submittedSubmissions = CurrentUser.IsMat
            ? await GetMatSubmittedSubmissions(sectionId)
            : await GetSchoolSubmittedSubmissions(sectionId);

        return new SelfAssessmentSummaryViewModel
        {
            SectionName = section.Name,
            CategoryName = categorySlug.Replace("-", " "),
            CompletedSchoolCount = submittedSubmissions.Count,
            IsMatSummary = CurrentUser.IsMat,
            RecommendationLinks = submittedSubmissions
                .Select(BuildRecommendationLink)
                .ToList(),
            ShowSubmitAnotherSelfAssessment = true,
            SubmitAnotherSelfAssessmentHref = CurrentUser.IsMat
                ? $"/groups/{UrlConstants.GroupSelfAssessmentSelectionSlug}"
                : $"/{categorySlug}",
            BackToHomeHref = UrlConstants.HomePage
        };

        SelfAssessmentSummaryRecommendationLinkViewModel BuildRecommendationLink(
            SqlSubmissionDto submission
        )
        {
            return new SelfAssessmentSummaryRecommendationLinkViewModel
            {
                LinkText = CurrentUser.IsMat
                    ? submission.Establishment!.OrgName
                    : $"View the recommendations for {section.Name!.ToLower()}",
                Href = CurrentUser.IsMat
                    ? $"/school/{categorySlug}/{sectionSlug}"
                    : $"/{categorySlug}"
            };
        }
    }

    private async Task<List<SqlSubmissionDto>> GetMatSubmittedSubmissions(
        string sectionId
    )
    {
        var selectedEstablishmentIds =
            _httpContextAccessor.HttpContext!.Session
                .GetSelectedEstablishmentIds()
                .ToArray();

        if (selectedEstablishmentIds.Length == 0)
        {
            var activeEstablishmentId =
                await GetActiveEstablishmentIdOrThrowException();

            selectedEstablishmentIds = [activeEstablishmentId];
        }

        var completedSubmissions =
            await _groupService.GetGroupCompletedSubmissionsBySections(
                selectedEstablishmentIds
            );

        return completedSubmissions
            .Where(s => s.SectionId == sectionId)
            .ToList();
    }

    private async Task<List<SqlSubmissionDto>> GetSchoolSubmittedSubmissions(
        string sectionId
    )
    {
        var establishmentId = await GetActiveEstablishmentIdOrThrowException();

        var completedSubmissions =
            await _groupService.GetGroupCompletedSubmissionsBySections(
                [establishmentId]
            );

        return completedSubmissions
            .Where(s => s.SectionId == sectionId)
            .ToList();
    }
}
