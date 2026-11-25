using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Web.ViewModels;

namespace Dfe.PlanTech.Web.ViewBuilders.Interfaces
{
    public interface ICategoryLandingViewComponentViewBuilder
    {
        Task<CategoryLandingViewComponentViewModel> BuildViewModelAsync(
            QuestionnaireCategoryEntry category,
            string slug,
            string? sectionName,
            string? sortOrder,
            bool print = false,
            CategoryLandingContext context = CategoryLandingContext.School
        );
    }
}
