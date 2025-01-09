using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Domain.Content.Models.ContentSupport;

[ExcludeFromCodeCoverage]
public class ContentSupportPage : Page
{
    public bool IsSitemap { get; init; }
    public bool HasCitation { get; init; }
    public bool HasBackToTop { get; init; }
    public bool HasFeedbackBanner { get; init; }
    public bool HasPrint { get; init; }
    public bool ShowVerticalNavigation { get; init; }
}
