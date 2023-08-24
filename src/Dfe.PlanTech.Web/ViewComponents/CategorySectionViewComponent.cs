using Dfe.PlanTech.Domain.CategorySection;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.TagHelpers.TaskList;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents
{
    public class CategorySectionViewComponent : ViewComponent
    {
        private readonly ILogger<CategorySectionViewComponent> _logger;

        public CategorySectionViewComponent(ILogger<CategorySectionViewComponent> logger)
        {
            _logger = logger;
        }

        public IViewComponentResult Invoke(ICategory category)
        {
            category.RetrieveSectionStatuses();

            var categorySectionViewModel = new CategorySectionViewComponentViewModel()
            {
                CompletedSectionCount = category.Completed,
                TotalSectionCount = category.Sections.Length,
                CategorySectionDto = _GetCategorySectionViewComponentViewModel(category),
                ProgressRetrievalErrorMessage = category.RetrievalError ? "Unable to retrieve progress, please refresh your browser." : null
            };

            return View(categorySectionViewModel);
        }

        private IEnumerable<CategorySectionDto> _GetCategorySectionViewComponentViewModel(ICategory category)
        {
            foreach (var categorySection in category.Sections)
            {
                var categorySectionDto = new CategorySectionDto()
                {
                    Slug = categorySection.InterstitialPage.Slug,
                    Name = categorySection.Name
                };

                if (string.IsNullOrWhiteSpace(categorySectionDto.Slug)) categorySectionDto.Slug = null;

                if (categorySectionDto.Slug != null)
                {
                    if (!category.RetrievalError)
                    {
                        var sectionStatusCompleted = category.SectionStatuses.FirstOrDefault(sectionStatus => sectionStatus.SectionId == categorySection.Sys.Id)?.Completed;

                        if (sectionStatusCompleted != null)
                        {
                            categorySectionDto.TagColour = (sectionStatusCompleted == 1 ? TagColour.DarkBlue : TagColour.Green).ToString();
                            categorySectionDto.TagText = sectionStatusCompleted == 1 ? "COMPLETE" : "STARTED";
                        }
                        else
                        {
                            categorySectionDto.TagColour = TagColour.Grey.ToString();
                            categorySectionDto.TagText = "NOT STARTED";
                        }
                    }
                    else
                    {
                        categorySectionDto.TagColour = TagColour.Red.ToString();
                        categorySectionDto.TagText = "Unable to retrieve status";
                    }

                    TempData[categorySectionDto.Slug] = categorySectionDto.Name + "+" + categorySection.Sys.Id;
                }
                else
                {
                    _logger.LogError("No Slug found for Subtopic with ID: {categorySectionId}", categorySection.Sys.Id);
                    categorySectionDto.NoSlugForSubtopicErrorMessage = String.Format("{0} unavailable", categorySection.Name);
                }

                yield return categorySectionDto;
            }
        }
    }
}