using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class PageViewModel
{
    public bool DisplayBlueBanner { get; set; } = true;
    public PageEntry Page { get; set; }
    public string? OrganisationName { get; set; }

    public PageViewModel(PageEntry page, bool displayBlueBanner = true)
    {
        DisplayBlueBanner = displayBlueBanner;
        Page = page;
    }
}
