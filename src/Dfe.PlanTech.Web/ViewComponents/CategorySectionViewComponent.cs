using Dfe.PlanTech.Domain.CategorySection;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.TagHelpers.TaskList;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class CategorySectionViewComponent : ViewComponent
{
    private readonly ILogger<CategorySectionViewComponent> _logger;
    private readonly IGetSubmissionStatusesQuery _query;


    public CategorySectionViewComponent(ILogger<CategorySectionViewComponent> logger, IGetSubmissionStatusesQuery query)
    {
        _logger = logger;
        _query = query;
    }

    public IViewComponentResult Invoke(Category category)
    {
        var viewModel = GenerateViewModel(category);

        return View(viewModel);
    }

    private CategorySectionViewComponentViewModel GenerateViewModel(Category category)
    {
        bool sectionsExist = category.Sections?.Count > 0;

        if (!sectionsExist)
        {
            _logger.LogError("Found no sections for category {id}", category.Sys.Id);

            return new CategorySectionViewComponentViewModel()
            {
                NoSectionsErrorRedirectUrl = "ServiceUnavailable"
            };
        }

        category = RetrieveSectionStatuses(category);

        return new CategorySectionViewComponentViewModel()
        {
            CompletedSectionCount = category.Completed,
            TotalSectionCount = category.Sections?.Count ?? 0,
            CategorySectionDto = GetCategorySectionViewComponentViewModel(category),
            ProgressRetrievalErrorMessage = category.RetrievalError ? "Unable to retrieve progress, please refresh your browser." : null
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
        categorySectionDto.TagColour = TagColour.Red.ToString();
        categorySectionDto.TagText = "UNABLE TO RETRIEVE STATUS";
    }

    private static void SetCategorySectionDtoTagWithCurrentStatus(Category category, Section categorySection, CategorySectionDto categorySectionDto)
    {
        var sectionStatusCompleted = category.SectionStatuses.FirstOrDefault(sectionStatus => sectionStatus.SectionId == categorySection.Sys.Id)?.Completed;

        if (sectionStatusCompleted != null)
        {
            categorySectionDto.TagColour = sectionStatusCompleted == 1 ? TagColour.Blue : TagColour.LightBlue;
            categorySectionDto.TagText = sectionStatusCompleted == 1 ? "COMPLETE" : "IN PROGRESS";
        }
        else
        {
            categorySectionDto.TagColour = TagColour.Grey.ToString();
            categorySectionDto.TagText = "NOT STARTED";
        }
    }

    private IEnumerable<CategorySectionDto> GetCategorySectionViewComponentViewModel(Category category)
    {
        foreach (var section in category.Sections)
        {
            var categorySectionDto = new CategorySectionDto()
            {
                Slug = section.InterstitialPage?.Slug,
                Name = section.Name
            };

            if (string.IsNullOrWhiteSpace(categorySectionDto.Slug)) LogErrorWithUserFeedback(section, categorySectionDto);
            else if (category.RetrievalError) SetCategorySectionDtoTagWithRetrievalError(categorySectionDto);
            else SetCategorySectionDtoTagWithCurrentStatus(category, section, categorySectionDto);

            yield return categorySectionDto;
        }
    }

    public Category RetrieveSectionStatuses(Category category)
    {
        try
        {
            category.SectionStatuses = _query.GetSectionSubmissionStatuses(category.Sections);
            category.Completed = category.SectionStatuses.Count(x => x.Completed == 1);
            category.RetrievalError = false;
            return category;
        }
        catch (Exception e)
        {
            _logger.LogError("An exception has occurred while trying to retrieve section progress with the following message - {}", e.Message);
            category.RetrievalError = true;
            return category;
        }
    }
}