using Dfe.PlanTech.Domain.CategorySection;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.TagHelpers.TaskList;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class CategorySectionViewComponent(
    ILogger<CategorySectionViewComponent> logger,
    IGetSubmissionStatusesQuery query,
    IGetSubTopicRecommendationQuery getSubTopicRecommendationQuery) : ViewComponent
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

        category = await RetrieveSectionStatuses(category);

        return new CategorySectionViewComponentViewModel
        {
            CompletedSectionCount = category.Completed,
            TotalSectionCount = category.Sections.Count,
            CategorySectionDto = await GetCategorySectionDto(category).ToListAsync(),
            ProgressRetrievalErrorMessage = category.RetrievalError
                ? "Unable to retrieve progress, please refresh your browser."
                : null
        };
    }

    private void LogErrorWithUserFeedback(Section categorySection, CategorySectionDto categorySectionDto)
    {
        categorySectionDto.Slug = null;
        _logger.LogError("No Slug found for Subtopic with ID: {categorySectionId}", categorySection.Sys.Id);
        categorySectionDto.NoSlugForSubtopicErrorMessage = string.Format("{0} unavailable", categorySection.Name);
    }

    private static void SetCategorySectionDtoTagWithRetrievalError(CategorySectionDto categorySectionDto)
    {
        categorySectionDto.TagColour = TagColour.Red;
        categorySectionDto.TagText = "UNABLE TO RETRIEVE STATUS";
    }

    private static void SetCategorySectionDtoTagWithCurrentStatus(
        Category category,
        Section categorySection,
        CategorySectionDto categorySectionDto)
    {
        var sectionStatusCompleted = category.SectionStatuses
            .FirstOrDefault(sectionStatus => sectionStatus.SectionId == categorySection.Sys.Id)?.Completed;

        if (sectionStatusCompleted != null)
        {
            categorySectionDto.TagColour = sectionStatusCompleted == 1 ? TagColour.Blue : TagColour.LightBlue;
            categorySectionDto.TagText = sectionStatusCompleted == 1 ? "COMPLETE" : "IN PROGRESS";
        }
        else
        {
            categorySectionDto.TagColour = TagColour.Grey;
            categorySectionDto.TagText = "NOT STARTED";
        }
    }
    
    private static void SetCategorySectionRecommendationDtoTag(CategorySectionRecommendationDto categorySectionRecommendationDto)
    {
        var recommendationReady = categorySectionRecommendationDto.RecommendationSlug != null;

        categorySectionRecommendationDto.TagColour = recommendationReady ? TagColour.Blue : TagColour.Grey;
        categorySectionRecommendationDto.TagText = recommendationReady ? "Ready" : "Not Available";
    }

    private async IAsyncEnumerable<CategorySectionDto> GetCategorySectionDto(Category category)
    {
        foreach (var section in category.Sections)
        {
            var sectionStatus = category.SectionStatuses.FirstOrDefault(sectionStatus =>
                sectionStatus.SectionId == section.Sys.Id && sectionStatus.Completed == 1);
            var recommendation = await GetSectionRecommendation(section, sectionStatus);
            SetCategorySectionRecommendationDtoTag(recommendation);

            var categorySectionDto = new CategorySectionDto
            {
                Slug = section.InterstitialPage.Slug,
                Name = section.Name,
                CategorySectionRecommendation = recommendation
            };

            if (string.IsNullOrWhiteSpace(categorySectionDto.Slug))
                LogErrorWithUserFeedback(section, categorySectionDto);
            else if (category.RetrievalError) SetCategorySectionDtoTagWithRetrievalError(categorySectionDto);
            else SetCategorySectionDtoTagWithCurrentStatus(category, section, categorySectionDto);

            yield return categorySectionDto;
        }
    }

    public async Task<Category> RetrieveSectionStatuses(Category category)
    {
        try
        {
            category.SectionStatuses = await _query.GetSectionSubmissionStatuses(category.Sys.Id);
            category.Completed = category.SectionStatuses.Count(x => x.Completed == 1);
            category.RetrievalError = false;
            return category;
        }
        catch (Exception e)
        {
            _logger.LogError(
                "An exception has occurred while trying to retrieve section progress with the following message - {message}",
                e.Message);
            category.RetrievalError = true;
            return category;
        }
    }

    private async Task<CategorySectionRecommendationDto> GetSectionRecommendation(ISectionComponent section, SectionStatusDto? sectionStatus)
    {
        var sectionMaturity = sectionStatus?.Maturity;

        if (string.IsNullOrEmpty(sectionMaturity)) return new CategorySectionRecommendationDto();

        var recommendation =
            await _getSubTopicRecommendationQuery.GetRecommendationsViewDto(section.Sys.Id, sectionMaturity);

        if (recommendation == null)
        {
            _logger.LogError("No Recommendation Found: Section - {sectionName}, Maturity - {sectionMaturity}", section.Name, sectionMaturity);

            return new CategorySectionRecommendationDto
            {
                NoRecommendationFoundErrorMessage = $"Unable to retrieve {section.Name} recommendation"
            };
        }

        if (section.InterstitialPage?.Slug == null)
        {
            _logger.LogError("No Slug found for Subtopic with ID: {SectionId}  / name: {SectionName}", section.Sys.Id, section.Name);
        }
        
        return new CategorySectionRecommendationDto
        {
            RecommendationSlug = recommendation.RecommendationSlug,
            RecommendationDisplayName = recommendation.DisplayName,
            SectionSlug = section.InterstitialPage?.Slug ?? "",
        };
    }

    public async Task<ICategoryComponent> RetrieveSectionStatuses(ICategoryComponent category)
    {
        try
        {
            category.SectionStatuses = await _query.GetSectionSubmissionStatuses(category.Sys.Id);
            category.Completed = category.SectionStatuses.Count(x => x.Completed == 1);
            category.RetrievalError = false;

            return category;
        }
        catch (Exception e)
        {
            _logger.LogError(
                "An exception has occurred while trying to retrieve section progress with the following message - {errorMessage}",
                e.Message);
            category.RetrievalError = true;
            return category;
        }
    }
}