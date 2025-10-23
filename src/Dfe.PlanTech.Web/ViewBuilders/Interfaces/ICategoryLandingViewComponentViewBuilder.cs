using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Web.ViewModels;

namespace Dfe.PlanTech.Web.ViewBuilders.Interfaces
{
    public interface ICategoryLandingViewComponentViewBuilder
    {
        Task<CategoryLandingViewComponentViewModel> BuildViewModelAsync(QuestionnaireCategoryEntry category, string slug, string? sectionName, string? sortOrder);
    }
}
