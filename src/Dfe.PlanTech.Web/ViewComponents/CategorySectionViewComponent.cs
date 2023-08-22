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
            var categorySectionViewModel = new CategorySectionViewComponentViewModel()
            {
                CompletedCount = category.SectionStatuses.Where(sectionStatus => sectionStatus.Completed == 1).ToList().Count,
                CategorySectionDto = _GetCategorySectionViewComponentViewModel(category)
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

                TempData[categorySectionDto.Slug] = categorySectionDto.Name + "+" + categorySection.Sys.Id;

                yield return categorySectionDto;
            }
        }
    }
}