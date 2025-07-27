using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Web.Context;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class FooterLinksViewComponentViewBuilder(
    ILogger<FooterLinksViewComponentViewBuilder> logger,
    CurrentUser currentUser,
    ContentfulService contentfulService
) : BaseViewBuilder(currentUser)
{
    private readonly ILogger<FooterLinksViewComponentViewBuilder> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ContentfulService _contentfulService = contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));

    public Task<List<CmsNavigationLinkDto>> GetNavigationLinksAsync()
    {
        try
        {
            return _contentfulService.GetNavigationLinks();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error retrieving navigation links for footer");
            return Task.FromResult(new List<CmsNavigationLinkDto>());
        }
    }
}
