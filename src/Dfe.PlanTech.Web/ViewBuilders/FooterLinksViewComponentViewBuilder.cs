using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Web.Context.Interfaces;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class FooterLinksViewComponentViewBuilder(
    ILogger<FooterLinksViewComponentViewBuilder> logger,
    IContentfulService contentfulService,
    ICurrentUser currentUser
) : BaseViewBuilder(logger, contentfulService, currentUser)
{
    public Task<List<NavigationLinkEntry>> GetNavigationLinksAsync()
    {
        try
        {
            return ContentfulService.GetNavigationLinksAsync();
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Error retrieving navigation links for footer");
            return Task.FromResult(new List<NavigationLinkEntry>());
        }
    }
}
