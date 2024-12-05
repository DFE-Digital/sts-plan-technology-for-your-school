using Dfe.PlanTech.Domain.Content.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Domain.Content.Models.ContentSupport;

[ExcludeFromCodeCoverage]
public class ContentSupportPage : ContentBase, IContentSupportPage<CSBodyText>
{
    public CSHeading Heading { get; init; } = null!;
    public List<CSBodyText> Content { get; init; } = [];
    public bool IsSitemap { get; init; }
    public bool HasCitation { get; init; }
    public bool HasBackToTop { get; init; }
    public bool HasFeedbackBanner { get; init; }
    public bool HasPrint { get; init; }
    public bool ShowVerticalNavigation { get; init; }
}