using Dfe.PlanTech.Domain.CategorySection;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.TagHelpers.TaskList;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents
{
    public class CategorySectionViewComponent : ViewComponent
    {
        private readonly ILogger<CategorySectionViewComponent> _logger;
        private readonly IGetSubmissionStatusesQuery _query;


        public CategorySectionViewComponent(ILogger<CategorySectionViewComponent> logger, IGetSubmissionStatusesQuery query)
        {
            _logger = logger;
            _query = query;
        }

        public IViewComponentResult Invoke(ICategory category)
        {
            bool sectionsExist = category.Sections != null && category.Sections?.Length > 0;

            category = RetrieveSectionStatuses(category);

            var categorySectionViewModel = new CategorySectionViewComponentViewModel()
            {
                CompletedSectionCount = category.Completed,
                TotalSectionCount = category.Sections?.Length ?? 0,
                CategorySectionDto = sectionsExist ? _GetCategorySectionViewComponentViewModel(category) : Enumerable.Empty<CategorySectionDto>(),
                ProgressRetrievalErrorMessage = category.RetrievalError ? "Unable to retrieve progress, please refresh your browser." : null
            };

            if (!sectionsExist) categorySectionViewModel.NoSectionsErrorRedirectUrl = "ServiceUnavailable";

            return View(categorySectionViewModel);
        }

        private void _LogErrorWithUserFeedback(ISection categorySection, ref CategorySectionDto categorySectionDto)
        {
            categorySectionDto.Slug = null;
            _logger.LogError("No Slug found for Subtopic with ID: {categorySectionId}", categorySection.Sys.Id);
            categorySectionDto.NoSlugForSubtopicErrorMessage = String.Format("{0} unavailable", categorySection.Name);
        }

        private static void _SetCategorySectionDtoTagWithRetrievalError(ref CategorySectionDto categorySectionDto)
        {
            categorySectionDto.TagColour = TagColour.Red.ToString();
            categorySectionDto.TagText = "UNABLE TO RETRIEVE STATUS";
        }

        private static void _SetCategorySectionDtoTagWithCurrentStatus(ICategory category, ISection categorySection, ref CategorySectionDto categorySectionDto)
        {
            var sectionStatusCompleted = category.SectionStatuses.FirstOrDefault(sectionStatus => sectionStatus.SectionId == categorySection.Sys.Id)?.Completed;

            if (sectionStatusCompleted != null)
            {
                categorySectionDto.TagColour = sectionStatusCompleted == 1 ? TagColour.DarkBlue.ToString() : TagColour.Blue.ToString();
                categorySectionDto.TagText = sectionStatusCompleted == 1 ? "COMPLETE" : "IN PROGRESS";
            }
            else
            {
                categorySectionDto.TagColour = TagColour.Grey.ToString();
                categorySectionDto.TagText = "NOT STARTED";
            }
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

                if (string.IsNullOrWhiteSpace(categorySectionDto.Slug)) _LogErrorWithUserFeedback(categorySection, ref categorySectionDto);
                else if (category.RetrievalError) _SetCategorySectionDtoTagWithRetrievalError(ref categorySectionDto);
                else _SetCategorySectionDtoTagWithCurrentStatus(category, categorySection, ref categorySectionDto);

                if (categorySectionDto.Slug != null) TempData[categorySectionDto.Slug] = categorySectionDto.Name + "+" + categorySection.Sys.Id + "+" + categorySectionDto.Slug;

                yield return categorySectionDto;
            }
        }
        
        public ICategory RetrieveSectionStatuses(ICategory category)
        {
            try
            {
                category.SectionStatuses = _query.GetSectionSubmissionStatuses(category.Sections).ToList();
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
}