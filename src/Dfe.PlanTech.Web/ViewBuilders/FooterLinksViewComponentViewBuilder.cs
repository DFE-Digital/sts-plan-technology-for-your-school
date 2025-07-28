using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Web.Context;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class FooterLinksViewComponentViewBuilder(
    ILoggerFactory loggerFactory,
    ContentfulService contentfulService,
    CurrentUser currentUser
) : BaseViewBuilder(loggerFactory, contentfulService, currentUser)
{
    private readonly ILogger<FooterLinksViewComponentViewBuilder> _logger = loggerFactory.CreateLogger<FooterLinksViewComponentViewBuilder>();

    public Task<List<CmsNavigationLinkDto>> GetNavigationLinksAsync()
    {
        try
        {
            return ContentfulService.GetNavigationLinks();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error retrieving navigation links for footer");
            return Task.FromResult(new List<CmsNavigationLinkDto>());
        }
    }
}
