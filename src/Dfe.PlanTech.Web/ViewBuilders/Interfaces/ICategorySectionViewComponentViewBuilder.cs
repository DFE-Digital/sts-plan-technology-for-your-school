using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Web.ViewModels;

namespace Dfe.PlanTech.Web.ViewBuilders.Interfaces
{
    public interface ICategorySectionViewComponentViewBuilder
    {
        Task<CategoryCardsViewComponentViewModel> BuildViewModelAsync(
            QuestionnaireCategoryEntry category,
            CategoryLandingContext context = CategoryLandingContext.School
        );
    }
}
