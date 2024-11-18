using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web.Models.Content;

[ExcludeFromCodeCoverage]
public class ContentSupportPage : ContentBase, IContentSupportPage<Entry>
{
    public Heading Heading { get; init; } = null!;
    public List<Entry> Content { get; init; } = [];
    public bool IsSitemap { get; init; }
    public bool HasCitation { get; init; }
    public bool HasBackToTop { get; init; }
    public bool HasFeedbackBanner { get; init; }
    public bool HasPrint { get; init; }
    public bool ShowVerticalNavigation { get; init; }
}
