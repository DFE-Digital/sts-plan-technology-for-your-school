using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface IContentSupportPage<TContent> : IContentSupportPage
where TContent : class
{
    List<TContent> Content { get; }
}

public interface IContentSupportPage : IContentComponent, IHasSlug
{
    CSHeading Heading { get; }
    bool IsSitemap { get; }
    bool HasCitation { get; }
    bool HasBackToTop { get; }
    bool HasFeedbackBanner { get; }
    bool HasPrint { get; }
    bool ShowVerticalNavigation { get; }
}