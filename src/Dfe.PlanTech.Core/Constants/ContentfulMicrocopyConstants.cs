using Contentful.Core.Models.Management;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
namespace Dfe.PlanTech.Core.Constants;

[ExcludeFromCodeCoverage]
public static class ContentfulMicrocopyConstants
{
    // Fallback text
    private const string EmptyFallback = "";

    // Home
    private const string CardsFallback = "Go to standard";

    // Landing page
    private const string TopicLinksFallback = "Start, continue or view answers for this self-assessment";
    private const string PrintLinksFallback = "Print all recommendations";
    private const string SuccessHeaderFallback = "Your self-assessment is complete";
    private const string LandingPageBackLinkFallback = "Back to choose a standard";

    // Groups select a school
    private const string GroupsSelectHeaderFallback = "Select a school";

    // Print views
    private const string PrintSectionIncompleteFallback = "The self-assessment must be completed before recommendations are available.";

    // Single recommendation
    private const string PrintSingleLinkFallback = "Print this recommendation";
    private const string StatusUpdatedFallback = "Status updated";
    private const string RecommendationStatusQuestionFallback = "Have you completed this recommendation?";
    private const string RecommendationStatusButtonFallback = "Update status";
    private const string RecommendationHistoryHeaderFallback = "Recent activity";
    private const string RecommendationCommentLabelFallback = "Add a comment";
    private const string RecommendationCommentHintFallback = "Anyone who uses the service for your school will be able to see comments. Do not include personal or sensitive information, like email addresses or account details.";
    private const string RecommendationHistoryInitialFallback = "Status set based on self assessment.";
    private const string RecommendationHistoryChangeFallback = "Status changed.";

    // View answers
    private const string ViewAnswersHeaderFallback = "View your answers";
    private const string ViewAnswersBackToRecsFallback = "Back to recommendations";

    // Check answers
    private const string CheckAnswersChangeLinkFallback = "Change";
    private const string CheckAnswersSubmitButtonFallback = "Submit and view recommendations";

    // Continue self-assessment
    private const string ContinueHeaderFallback = "Continue self-assessment";
    private const string ContinueAnswersHeaderFallback = "Your self-assessment answers";
    private const string ContinueContinueButtonFallback = "Continue self-assessment";
    private const string ContinueRestartButtonFallback = "Restart self-assessment";

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

    // Single recommendation page
    public static readonly MicrocopyRecord SingleRecommendationPrintLink = new("printLinkSingle", PrintSingleLinkFallback);
    public static readonly MicrocopyRecord SingleRecommendationPrintAllLink = new("printLinkTopic", PrintLinksFallback, "topic");
    public static readonly MicrocopyRecord SingleRecommendationSuccessHeader = new("successStatus", StatusUpdatedFallback, "status");
    public static readonly MicrocopyRecord SingleRecommendationPosition = new("recommendationPosition", EmptyFallback, "position", "totalRecsForTopic");
    public static readonly MicrocopyRecord SingleRecommendationStatusQuestion = new("recommendationStatusQuestions", RecommendationStatusQuestionFallback);
    public static readonly MicrocopyRecord SingleRecommendationStatusButton = new("recommendationStatusButton", RecommendationStatusButtonFallback);
    public static readonly MicrocopyRecord SingleRecommendationHistoryHeader = new("recommendationHistoryHeader", RecommendationHistoryHeaderFallback);
    public static readonly MicrocopyRecord SingleRecommendationStatusHint = new("recommendationStatusHint", EmptyFallback);
    public static readonly MicrocopyRecord SingleRecommendationCommentLabel = new("recommendationCommentLabel", RecommendationCommentLabelFallback);
    public static readonly MicrocopyRecord SingleRecommendationCommentHint = new("recommendationCommentHint", RecommendationCommentHintFallback);
    public static readonly MicrocopyRecord SingleRecommendationHistoryInitial = new("recommendationHistoryInitialStatus", RecommendationHistoryInitialFallback, "recStatus", "establishment");
    public static readonly MicrocopyRecord SingleRecommendationHistoryAnswer = new("recommendationHistoryAnswer", EmptyFallback, "answer");
    public static readonly MicrocopyRecord SingleRecommendationHistoryChange = new("recommendationHistoryChange", RecommendationHistoryChangeFallback, "recStatus");
    public static readonly MicrocopyRecord SingleRecommendationHistoryReason = new("recommendationHistoryReason", EmptyFallback, "recStatus");

    // View your answers
    public static readonly MicrocopyRecord ViewAnswersHeader = new("answersHeader", ViewAnswersHeaderFallback);
    public static readonly MicrocopyRecord ViewAnswersIntroText = new("answersIntroText", EmptyFallback, "dateCompleted");
    public static readonly MicrocopyRecord ViewAnswersIntroMore = new("answersIntroMore", EmptyFallback);
    public static readonly MicrocopyRecord ViewAnswersBackToRecs = new("answersBackButton", ViewAnswersBackToRecsFallback);

    // Continue self-assessment
    public static readonly MicrocopyRecord ContinueHeader = new("continueHeader", ContinueHeaderFallback);
    public static readonly MicrocopyRecord ContinueIntroText = new("continueIntroText", EmptyFallback, "dateUpdated");
    public static readonly MicrocopyRecord ContinueAnswersHeader = new("continueAnswersHeader", ContinueAnswersHeaderFallback);
    public static readonly MicrocopyRecord ContinueInfoText = new("continueInfoText", EmptyFallback);
    public static readonly MicrocopyRecord ContinueInfoMore = new("continueInfoMore", EmptyFallback);
    public static readonly MicrocopyRecord ContinueContinueButton = new("continueContinueButton", ContinueContinueButtonFallback);
    public static readonly MicrocopyRecord ContinueRestartButton = new("continueRestartButton", ContinueRestartButtonFallback);

    // Check answers (during self-assessment)
    public static readonly MicrocopyRecord CheckAnswersChangeLink = new("checkAnswersChangeLink", CheckAnswersChangeLinkFallback);
    public static readonly MicrocopyRecord CheckAnswersSubmitButton = new("checkAnswersSubmitButton", CheckAnswersSubmitButtonFallback);

    // Print page (standard/category)
    public static readonly MicrocopyRecord CateogoryPrintSectionNotStarted = new("printNotStarted", PrintSectionIncompleteFallback);
    public static readonly MicrocopyRecord CategoryPrintSectionInProgress = new("printContinue", PrintSectionIncompleteFallback, "dateUpdated");
    public static readonly MicrocopyRecord CategoryPrintSectionCompleted = new("printCompleted", EmptyFallback, "topic", "dateCompleted");

    // Groups - select a school
    public static readonly MicrocopyRecord GroupsSelectHeader = new("matSelect", GroupsSelectHeaderFallback);
    public static readonly MicrocopyRecord GroupsSelectRecommendationCount = new("matCount", EmptyFallback, "count", "total");
    public static readonly MicrocopyRecord GroupsSelectContactUs = new("matContact", EmptyFallback, "contactLink");
}
