using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Constants;

[ExcludeFromCodeCoverage]
public static class ContentfulMicrocopyConstants
{
    public record MicrocopyRecord(string Key, params string[] Variables);

    // Home
    public static readonly MicrocopyRecord HomeHeader = new("homeHeader");
    public static readonly MicrocopyRecord HomeCardStatusSingleNotStarted = new("cardStatusSingleNotStarted");
    public static readonly MicrocopyRecord HomeCardStatusMultipleNotStarted = new("cardStatusMultipleNotStarted");
    public static readonly MicrocopyRecord HomeCardStatusContinue = new("cardStatusContinue");
    public static readonly MicrocopyRecord HomeCardStatusViewRecommendations = new("cardStatusViewRecommendations");

    // Landing Page
    public static readonly MicrocopyRecord LandingPageHeader = new("landingHeader");
    public static readonly MicrocopyRecord LandingPageBackLink = new("landingBackLink");
    public static readonly MicrocopyRecord LandingPagePrintLink = new("landingPrintLink", "standard");
    public static readonly MicrocopyRecord LandingPageInsetIntroNotStarted = new("insetIntroNotStarted", "standard");
    public static readonly MicrocopyRecord LandingPageInsetIntroContinue = new("insetIntroContinue", "standard");
    public static readonly MicrocopyRecord LandingPageTopicLinkNotStarted = new("topicLinkNotStarted", "topic");
    public static readonly MicrocopyRecord LandingPageTopicIntroContinue = new("topicIntroContinue", "dateUpdated");
    public static readonly MicrocopyRecord LandingPageTopicLinkContinue = new("topicLinkContinue", "topic");
    public static readonly MicrocopyRecord LandingPageTopicIntroCompleted = new("topicIntroCompleted", "topic", "dateCompleted");
    public static readonly MicrocopyRecord LandingPageTopicLinkCompleted = new("topicLinkCompleted", "topic");
    public static readonly MicrocopyRecord LandingPageSuccessPanelHeader = new("successHeader", "topic");
    public static readonly MicrocopyRecord LandingPageSuccessPanelBody = new("successBody");

    public static readonly List<string> EmptyFallback = new()
    {
        HomeHeader.Key,
        LandingPageHeader.Key,
        LandingPageInsetIntroNotStarted.Key,
        LandingPageInsetIntroContinue.Key,
        LandingPageTopicIntroContinue.Key,
        LandingPageTopicIntroCompleted.Key,
        LandingPageSuccessPanelBody.Key,
    };

    public static readonly List<string> CardsFallback = new()
    {
        HomeCardStatusSingleNotStarted.Key,
        HomeCardStatusMultipleNotStarted.Key,
        HomeCardStatusContinue.Key,
        HomeCardStatusViewRecommendations.Key,
    };

    public static readonly List<string> TopicLinksFallback = new()
    {
        LandingPageTopicLinkNotStarted.Key,
        LandingPageTopicLinkContinue.Key,
        LandingPageTopicLinkCompleted.Key
    };

    public static readonly List<string> PrintLinksFallback = new()
    {
        LandingPagePrintLink.Key
    };
}
