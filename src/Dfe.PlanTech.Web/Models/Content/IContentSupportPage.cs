using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Web.Models.Content;

public interface IContentSupportPage<TContent> : IContentSupportPage
where TContent : class
{
    List<TContent> Content { get; }
}

public interface IContentSupportPage : IContentComponent, IHasSlug
{
    Heading Heading { get; }
    bool IsSitemap { get; }
    bool HasCitation { get; }
    bool HasBackToTop { get; }
    bool HasFeedbackBanner { get; }
    bool HasPrint { get; }
    bool ShowVerticalNavigation { get; }
}
