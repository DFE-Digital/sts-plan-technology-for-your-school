using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Constants;

[ExcludeFromCodeCoverage]
public static class ContentfulMicrocopyConstants
{
    // Organised by view and/or component. Values correspond to Key field on Contentful microcopy entries.

    // Home
    public const string HomeHeader = "homeHeader";
    public const string HomeCardStatusSingleNotStarted = "cardStatusSingleNotStarted";
    public const string HomeCardStatusMultipleNotStarted = "cardStatusMultipleNotStarted";
    public const string HomeCardStatusContinue = "cardStatusContinue";
    public const string HomeCardStatusViewRecommendations = "cardStatusViewRecommendations";

    // Landing page
    public const string LandingPageHeader = "landingHeader";
    public const string LandingPageBackLink = "landingBackLink";
    public const string LandingPagePrintLink = "landingPrintLink"; // Variable - standard
    public const string LandingPageInsetIntroNotStarted = "insetIntroNotStarted"; // Variable - standard
    public const string LandingPageInsetIntroContinue = "insetIntroContinue"; // Variable - standard
    public const string LandingPageTopicLinkNotStarted = "topicLinkNotStarted"; // Variable - topic
    public const string LandingPageTopicIntroContinue = "topicIntroContinue"; // Variable - date
    public const string LandingPageTopicLinkContinue = "topicLinkContinue"; // Variable - topic
    public const string LandingPageTopicIntroCompleted = "topicIntroCompleted"; // Variables - topic, date
    public const string LandingPageTopicLinkCompleted = "topicLinkCompleted"; // Variable - topic
    public const string LandingPageSuccessPanelHeader = "successHeader"; // Variable - topic
    public const string LandingPageSuccessPanelBody = "successBody";

    public static readonly List<string> EmptyFallback = new()
    {
        HomeHeader,
        LandingPageHeader,
        LandingPageInsetIntroNotStarted,
        LandingPageInsetIntroContinue,
        LandingPageTopicIntroContinue,
        LandingPageTopicIntroCompleted,
        LandingPageSuccessPanelBody,
    };

    public static readonly List<string> CardsFallback = new()
    {
        HomeCardStatusSingleNotStarted,
        HomeCardStatusMultipleNotStarted,
        HomeCardStatusContinue,
        HomeCardStatusViewRecommendations,
    };

    public static readonly List<string> TopicLinksFallback = new()
    {
        LandingPageTopicLinkNotStarted,
        LandingPageTopicLinkContinue,
        LandingPageTopicLinkCompleted
    };

    public static readonly List<string> PrintLinksFallback = new()
    {
        LandingPagePrintLink
    };
}
