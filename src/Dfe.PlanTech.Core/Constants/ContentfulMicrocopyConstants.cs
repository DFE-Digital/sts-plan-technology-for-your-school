using Contentful.Core.Models.Management;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
namespace Dfe.PlanTech.Core.Constants;

[ExcludeFromCodeCoverage]
public static class ContentfulMicrocopyConstants
{
    // Fallback text
    private const string EmptyFallback = "";
    private const string CardsFallback = "Go to standard";
    private const string TopicLinksFallback = "Start, continue or view answers for this self-assessment";
    private const string PrintLinksFallback = "Print all recommendations";
    private const string SuccessHeaderFallback = "Your self-assessment is complete";
    private const string LandingPageBackLinkFallback = "Back to choose a standard";
    private const string GroupsSelectHeaderFallback = "Select a school";
    private const string PrintSectionIncompleteFallback = "The self-assessment must be completed before recommendations are available.";
    private const string PrintSingleLinkFallback = "Print this recommendation";
    private const string StatusUpdatedFallback = "Status updated";

    public record MicrocopyRecord
    {
        public string Key { get; init; }
        public string FallbackText { get; init; }
        public string[] Variables { get; init; }

        public MicrocopyRecord(string key, string fallbackText, params string[] variables)
        {
            Key = key;
            FallbackText = fallbackText;
            Variables = variables;
        }
    }

    // Home
    public static readonly MicrocopyRecord HomeHeader = new("homeHeader", EmptyFallback);
    public static readonly MicrocopyRecord HomeCardStatusSingleNotStarted = new("cardStatusSingleNotStarted", CardsFallback);
    public static readonly MicrocopyRecord HomeCardStatusMultipleNotStarted = new("cardStatusMultipleNotStarted", CardsFallback);
    public static readonly MicrocopyRecord HomeCardStatusContinue = new("cardStatusContinue", CardsFallback);
    public static readonly MicrocopyRecord HomeCardStatusViewRecommendations = new("cardStatusViewRecommendations", CardsFallback);

    // Landing Page
    public static readonly MicrocopyRecord LandingPageHeader = new("landingHeader", EmptyFallback);
    public static readonly MicrocopyRecord LandingPageBackLink = new("landingBackLink", LandingPageBackLinkFallback);
    public static readonly MicrocopyRecord LandingPagePrintLink = new("landingPrintLink", PrintLinksFallback, "standard");
    public static readonly MicrocopyRecord LandingPageInsetIntroNotStarted = new("insetIntroNotStarted", EmptyFallback, "standard");
    public static readonly MicrocopyRecord LandingPageInsetIntroContinue = new("insetIntroContinue", EmptyFallback, "standard");
    public static readonly MicrocopyRecord LandingPageTopicLinkNotStarted = new("topicLinkNotStarted", TopicLinksFallback, "topic");
    public static readonly MicrocopyRecord LandingPageTopicIntroContinue = new("topicIntroContinue", EmptyFallback, "dateUpdated");
    public static readonly MicrocopyRecord LandingPageTopicLinkContinue = new("topicLinkContinue", TopicLinksFallback, "topic");
    public static readonly MicrocopyRecord LandingPageTopicIntroCompleted = new("topicIntroCompleted", EmptyFallback, "topic", "dateCompleted");
    public static readonly MicrocopyRecord LandingPageTopicLinkCompleted = new("topicLinkCompleted", TopicLinksFallback, "topic");
    public static readonly MicrocopyRecord LandingPageSuccessPanelHeader = new("successHeader", SuccessHeaderFallback, "topic");
    public static readonly MicrocopyRecord LandingPageSuccessPanelBody = new("successBody", EmptyFallback);

    // Groups - select a school
    public static readonly MicrocopyRecord GroupsSelectHeader = new("matSelect", GroupsSelectHeaderFallback);
    public static readonly MicrocopyRecord GroupsSelectRecommendationCount = new("matCount", EmptyFallback, "count", "total");
    public static readonly MicrocopyRecord GroupsSelectContactUs = new("matContact", EmptyFallback, "contactLink");
}
