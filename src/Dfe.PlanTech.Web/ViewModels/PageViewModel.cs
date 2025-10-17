using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class PageViewModel
{
    public bool DisplayBlueBanner { get; set; } = true;
    public PageEntry Page { get; set; }
    public string? ActiveEstablishmentName { get; set; }
    public string? ActiveEstablishmentUrn { get; set; } // Note: not required/requested but useful for interim testing during development of this feature - we'll remove later

    public PageViewModel(PageEntry page, bool displayBlueBanner = true)
    {
        DisplayBlueBanner = displayBlueBanner;
        Page = page;
    }
}
