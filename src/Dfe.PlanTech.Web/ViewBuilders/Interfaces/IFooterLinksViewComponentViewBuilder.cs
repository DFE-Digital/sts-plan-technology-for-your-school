using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewBuilders.Interfaces
{
    public interface IFooterLinksViewComponentViewBuilder
    {
        Task<List<NavigationLinkEntry>> GetNavigationLinksAsync();
    }
}
