using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Web.ViewModels;

namespace Dfe.PlanTech.Web.ViewBuilders.Interfaces
{
    public interface IGroupsDashboardViewComponentViewBuilder
    {
        Task<GroupsDashboardViewComponentViewModel> BuildViewModelAsync(QuestionnaireCategoryEntry category);
    }
}
