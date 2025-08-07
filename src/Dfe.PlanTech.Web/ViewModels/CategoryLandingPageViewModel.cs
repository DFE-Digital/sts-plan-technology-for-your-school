using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Web.ViewModels;

public class CategoryLandingPageViewModel
{
    public CmsCategoryDto Category { get; set; } = null!;
    public string? SectionName { get; set; }
    public string Slug { get; set; } = null!;
    public CmsComponentTitleDto Title { get; set; } = null!;
}
