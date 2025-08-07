using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.CategorySection;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class CategorySectionViewComponent(
    ILogger<CategorySectionViewComponent> logger,
    IGetSubmissionStatusesQuery query,
    IGetSubTopicRecommendationQuery getSubTopicRecommendationQuery,
[FromServices] ISystemTime systemTime) : ViewComponent
{
    private readonly ILogger<CategorySectionViewComponent> _logger = logger;
    private readonly IGetSubmissionStatusesQuery _query = query;
    private readonly IGetSubTopicRecommendationQuery _getSubTopicRecommendationQuery = getSubTopicRecommendationQuery;

    public async Task<IViewComponentResult> InvokeAsync(Category category)
    {
        var viewModel = await GenerateViewModel(category);

        return View(viewModel);
    }

    private async Task<CategorySectionViewComponentViewModel> GenerateViewModel(Category category)
    {
        if (category.Sections.Count == 0)
        {
            _logger.LogError("Found no sections for category {id}", category.Sys.Id);

            return new CategorySectionViewComponentViewModel
            {
                NoSectionsErrorRedirectUrl = "ServiceUnavailable"
            };
        }

        category = await SubmissionStatusHelpers.RetrieveSectionStatuses(category, _logger, _query);
        var categoryLandingSlug = GetLandingPageSlug(category);

        return new CategorySectionViewComponentViewModel
        {
            Description = category.Content is { Count: > 0 } content ? content[0] : new MissingComponent(),
            CategoryHeaderText = category.Header.Text,
            CompletedSectionCount = category.Completed,
            TotalSectionCount = category.Sections.Count,
            CategorySlug = categoryLandingSlug,
            CategorySectionDto = await GetCategorySectionDto(category).ToListAsync(),
            ProgressRetrievalErrorMessage = category.RetrievalError
                ? "Unable to retrieve progress, please refresh your browser."
                : null
        };
    }

    private async IAsyncEnumerable<CategorySectionDto> GetCategorySectionDto(Category category)
    {
        foreach (var section in category.Sections)
        {
            var sectionStatus = category.SectionStatuses.FirstOrDefault(sectionStatus => sectionStatus.SectionId == section.Sys.Id);

            if (string.IsNullOrWhiteSpace(section.InterstitialPage?.Slug))
                _logger.LogError("No Slug found for Subtopic with ID: {sectionId}/ name: {sectionName}", section.Sys.Id, section.Name);

            yield return new CategorySectionDto(
                slug: section.InterstitialPage?.Slug,
                name: section.Name,
                retrievalError: category.RetrievalError,
                sectionStatus: sectionStatus,
                recommendation: await GetCategorySectionRecommendationDto(section, sectionStatus),
                systemTime
            );
        }
    }

    private async Task<CategorySectionRecommendationDto> GetCategorySectionRecommendationDto(ISectionComponent section, SectionStatusDto? sectionStatus)
    {
        if (string.IsNullOrEmpty(sectionStatus?.LastMaturity))
            return new CategorySectionRecommendationDto();

        try
        {
            var recommendation = await _getSubTopicRecommendationQuery.GetRecommendationsViewDto(section.Sys.Id, sectionStatus.LastMaturity);
            if (recommendation == null)
            {
                return new CategorySectionRecommendationDto
                {
                    NoRecommendationFoundErrorMessage = $"Unable to retrieve {section.Name} recommendation"
                };
            }
            return new CategorySectionRecommendationDto
            {
                RecommendationSlug = recommendation.RecommendationSlug,
                RecommendationDisplayName = recommendation.DisplayName,
                SectionSlug = section.InterstitialPage?.Slug,
                Viewed = sectionStatus.Viewed
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                             "An exception has occurred while trying to retrieve the recommendation for Section {sectionName}, with the message {errMessage}",
                             section.Name,
                             e.Message);
            return new CategorySectionRecommendationDto
            {
                NoRecommendationFoundErrorMessage = $"Unable to retrieve {section.Name} recommendation"
            };
        }
    }

    private static string GetLandingPageSlug(Category category)
    {
        if (category?.LandingPage?.Slug is string slug)
        {
            return slug;
        }

        throw new ContentfulDataUnavailableException($"Could not find category landing slug for category {category?.InternalName ?? "unknown"}");
    }
}
