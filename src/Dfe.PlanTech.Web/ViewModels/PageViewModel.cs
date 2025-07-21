using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Web.Models;

public class PageViewModel
{
    public bool DisplayBlueBanner { get; set; } = true;
    public CmsPageDto Page { get; set; }
    public string? OrganisationName { get; set; }

    public PageViewModel(CmsPageDto page, bool displayBlueBanner = true)
    {
        DisplayBlueBanner = displayBlueBanner;
        Page = page;
    }
}
