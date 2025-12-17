using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Dfe.PlanTech.Core.Constants;

[ExcludeFromCodeCoverage]
public static class ContentfulResourceConstants
{
    // Organised by view and/or component
    // Home
    public const string HomeHeader = "homeHeader";
    public const string HomeCardStatusSingleNotStarted = "cardStatusSingleNotstarted";
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

    private static readonly List<string> EmptyFallback = new()
    {
        HomeHeader,
        LandingPageHeader,
        LandingPageInsetIntroNotStarted,
        LandingPageInsetIntroContinue,
        LandingPageTopicIntroContinue,
        LandingPageTopicIntroCompleted
    };

    private static readonly List<string> CardsFallback = new()
    {
        HomeCardStatusSingleNotStarted,
        HomeCardStatusMultipleNotStarted,
        HomeCardStatusContinue,
        HomeCardStatusViewRecommendations,
    };

    private static readonly List<string> TopicLinksFallback = new()
    {
        LandingPageTopicLinkNotStarted,
        LandingPageTopicLinkContinue,
        LandingPageTopicLinkCompleted
    };

    private static readonly List<string> PrintLinksFallback = new()
    {
        LandingPagePrintLink
    };

    // Get fallback text
    public static string GetFallbackText(string intendedText)
    {
        if (EmptyFallback.Contains(intendedText))
        {
            return string.Empty;
        }
        else if (CardsFallback.Contains(intendedText))
        {
            return "Go to standard";
        }
        else if (TopicLinksFallback.Contains(intendedText))
        {
            return "Start, continue or view answers for this self-assessment";
        }
        else if (PrintLinksFallback.Contains(intendedText))
        {
            return "Print all recommendations";
        }
        else
        {
            return string.Empty;
            //throw??
        }
    }
}
