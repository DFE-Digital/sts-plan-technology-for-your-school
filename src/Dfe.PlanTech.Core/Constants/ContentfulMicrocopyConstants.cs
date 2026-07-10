using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Constants;

[ExcludeFromCodeCoverage]
public static class ContentfulMicrocopyConstants
{
    // Keys
    public const string HomeHeader = "homeHeader";
    public const string HomeCardStatusSingleNotStarted = "cardStatusSingleNotStarted";
    public const string HomeCardStatusMultipleNotStarted = "cardStatusMultipleNotStarted";
    public const string HomeCardStatusContinue = "cardStatusContinue";
    public const string HomeCardStatusViewRecommendations = "cardStatusViewRecommendations";

    public const string LandingPageHeader = "landingHeader";
    public const string LandingPageBackLink = "landingBackLink";
    public const string LandingPagePrintLink = "landingPrintLink";
    public const string LandingPageInsetIntroNotStarted = "insetIntroNotStarted";
    public const string LandingPageInsetIntroContinue = "insetIntroContinue";
    public const string LandingPageTopicLinkNotStarted = "topicLinkNotStarted";
    public const string LandingPageTopicIntroContinue = "topicIntroContinue";
    public const string LandingPageTopicLinkContinue = "topicLinkContinue";
    public const string LandingPageTopicIntroCompleted = "topicIntroCompleted";
    public const string LandingPageTopicLinkCompleted = "topicLinkCompleted";
    public const string LandingPageSuccessPanelHeader = "successHeader";
    public const string LandingPageSuccessPanelBody = "successBody";

    public const string SingleRecommendationPrintLink = "printLinkSingle";
    public const string SingleRecommendationPrintAllLink = "printLinkTopic";
    public const string SingleRecommendationSuccessHeader = "successStatus";
    public const string SingleRecommendationPosition = "recommendationPosition";
    public const string SingleRecommendationStatusQuestion = "recommendationStatusQuestions";
    public const string SingleRecommendationStatusButton = "recommendationStatusButton";
    public const string SingleRecommendationHistoryHeader = "recommendationHistoryHeader";
    public const string SingleRecommendationStatusHint = "recommendationStatusHint";
    public const string SingleRecommendationCommentLabel = "recommendationCommentLabel";
    public const string SingleRecommendationCommentHint = "recommendationCommentHint";
    public const string SingleRecommendationHistoryInitial = "recommendationHistoryInitialStatus";
    public const string SingleRecommendationHistoryAnswer = "recommendationHistoryAnswer";
    public const string SingleRecommendationHistoryChange = "recommendationHistoryChange";
    public const string SingleRecommendationHistoryReason = "recommendationHistoryReason";

    public const string ViewAnswersHeader = "answersHeader";
    public const string ViewAnswersIntroText = "answersIntroText";
    public const string ViewAnswersIntroMore = "answersIntroMore";
    public const string ViewAnswersBackToRecs = "answersBackButton";

    public const string ContinueHeader = "continueHeader";
    public const string ContinueIntroText = "continueIntroText";
    public const string ContinueAnswersHeader = "continueAnswersHeader";
    public const string ContinueInfoText = "continueInfoText";
    public const string ContinueInfoMore = "continueInfoMore";
    public const string ContinueContinueButton = "continueContinueButton";
    public const string ContinueRestartButton = "continueRestartButton";

    public const string CheckAnswersChangeLink = "checkAnswersChangeLink";
    public const string CheckAnswersSubmitButton = "checkAnswersSubmitButton";

    public const string CateogoryPrintSectionNotStarted = "printNotStarted";
    public const string CategoryPrintSectionInProgress = "printContinue";
    public const string CategoryPrintSectionCompleted = "printCompleted";

    public const string GroupsSelectHeader = "matSelect";
    public const string GroupsSelectRecommendationCount = "matCount";
    public const string GroupsSelectContactUs = "matContact";

    public const string SchoolSummarySuccessBody = "schoolSummarySuccessBody";
    public const string SchoolSummaryHeader = "summaryHeader";
    public const string SchoolSummaryRecLink = "schoolSummaryRecLink";
    public const string SchoolSummarySubmitAnotherBody = "schoolSummarySubmitAnotherBody";
    public const string MatSummarySuccessBody = "matSummarySuccessBody";
    public const string MatSummaryRecLinkIntro = "matSummaryRecLinkIntro";
    public const string MatSummarySubmitAnotherBody = "matSummarySubmitAnotherBody";
    public const string SummaryRecHeader = "summaryRecHeader";
    public const string SummaryRecBody = "summaryRecBody";
    public const string SummarySubmitAnotherHeading = "summarySubmitAnotherHeading";

    public const string msmTopicStartHeading = "msmTopicStartHeading";
    public const string msmTopicStartText1 = "msmTopicStartText1";
    public const string msmTopicStartText2 = "msmTopicStartText2";

    public const string GroupsSelectSchoolsToAssessHeader = "msmSelectHeader";
    public const string GroupsSelectSchoolsToAssessBackLink = "msmSelectBack";
    public const string GroupsSelectSchoolsToAssessHint = "msmSelectHint";
    public const string GroupsSelectSchoolsToAssessSchoolHint = "msmSelectSchoolHint";
    public const string GroupsSelectSchoolsToAssessCheckboxAll = "msmSelectCheckboxAll";
    public const string GroupsSelectSchoolsToAssessWarning = "msmSelectWarning";
    public const string GroupsSelectSchoolsToAssessContinue = "msmSelectSchoolContinue";
    public const string GroupsSelectSchoolsToAssessNoSelectionError = "msmErrorNoSelection";
    public const string GroupsSelectSchoolsToAssessConflictError = "msmErrorConflictingSelection";

    // Fallback text
    private const string EmptyFallback = "";

    private const string CardsFallback = "Go to standard";

    private const string TopicLinksFallback =
        "Start, continue or view answers for this self-assessment";
    private const string PrintLinksFallback = "Print all recommendations";
    private const string SuccessHeaderFallback = "Your self-assessment is complete";
    private const string LandingPageBackLinkFallback = "Back to choose a standard";

    private const string GroupsSelectHeaderFallback = "Select a school";

    private const string PrintSectionIncompleteFallback =
        "The self-assessment must be completed before recommendations are available.";

    private const string PrintSingleLinkFallback = "Print this recommendation";
    private const string StatusUpdatedFallback = "Recommendation updated";
    private const string RecommendationStatusQuestionFallback =
        "Have you completed this recommendation?";
    private const string RecommendationStatusButtonFallback = "Update status";
    private const string RecommendationHistoryHeaderFallback = "Recent activity";
    private const string RecommendationCommentLabelFallback = "Add a comment";
    private const string RecommendationCommentHintFallback =
        "Anyone who uses the service for your school will be able to see comments. Do not include personal or sensitive information, like email addresses or account details.";
    private const string RecommendationHistoryInitialFallback =
        "Status set based on self assessment.";
    private const string RecommendationHistoryChangeFallback = "Status changed.";

    private const string ViewAnswersHeaderFallback = "View your answers";
    private const string ViewAnswersBackToRecsFallback = "Back to recommendations";

    private const string CheckAnswersChangeLinkFallback = "Change";
    private const string CheckAnswersSubmitButtonFallback = "Submit and view recommendations";

    private const string ContinueHeaderFallback = "Continue self-assessment";
    private const string ContinueAnswersHeaderFallback = "Your self-assessment answers";
    private const string ContinueContinueButtonFallback = "Continue self-assessment";
    private const string ContinueRestartButtonFallback = "Restart self-assessment";

    private const string MsmTopicStartHeadingFallback = "Submit a self-assessment for your schools";
    private const string MsmTopicStartText1Fallback = "You can submit the self-assessment for one or more schools.";
    private const string MsmTopicStartText2Fallback = "If a self-assessment has been started but not submitted for a school, you can check their answers or submit a new self-assessment for them.";

    private const string GroupsSelectSchoolsToAssessBackLinkFallback = "Back";
    private const string GroupsSelectSchoolsToAssessHeaderFallback = "Which schools do you want to submit the self-assessment for?";
    private const string GroupsSelectSchoolsToAssessCheckboxAllFallback = "Submit self-assessment for all schools without a submission";
    private const string GroupsSelectSchoolsToAssessWarningFallback = "Starting a new self-assessment will replace any previous answers.";
    private const string GroupsSelectSchoolsToAssessContinueFallback = "Continue";
    private const string GroupsSelectSchoolsToAssessNoSelectionErrorFallback = "Select one, more or all schools";
    private const string GroupsSelectSchoolsToAssessConflictErrorFallback = "Select one or more schools, or select 'Submit self-assessment for all schools without a submission'";

    public static readonly IReadOnlyDictionary<string, string> FallbackText = new Dictionary<
        string,
        string
    >
    {
        { HomeHeader, EmptyFallback },
        { HomeCardStatusSingleNotStarted, CardsFallback },
        { HomeCardStatusMultipleNotStarted, CardsFallback },
        { HomeCardStatusContinue, CardsFallback },
        { HomeCardStatusViewRecommendations, CardsFallback },
        { LandingPageHeader, EmptyFallback },
        { LandingPageBackLink, LandingPageBackLinkFallback },
        { LandingPagePrintLink, PrintLinksFallback },
        { LandingPageInsetIntroNotStarted, EmptyFallback },
        { LandingPageInsetIntroContinue, EmptyFallback },
        { LandingPageTopicLinkNotStarted, TopicLinksFallback },
        { LandingPageTopicIntroContinue, EmptyFallback },
        { LandingPageTopicLinkContinue, TopicLinksFallback },
        { LandingPageTopicIntroCompleted, EmptyFallback },
        { LandingPageTopicLinkCompleted, TopicLinksFallback },
        { LandingPageSuccessPanelHeader, SuccessHeaderFallback },
        { LandingPageSuccessPanelBody, EmptyFallback },
        { SingleRecommendationPrintLink, PrintSingleLinkFallback },
        { SingleRecommendationPrintAllLink, PrintLinksFallback },
        { SingleRecommendationSuccessHeader, StatusUpdatedFallback },
        { SingleRecommendationPosition, EmptyFallback },
        { SingleRecommendationStatusQuestion, RecommendationStatusQuestionFallback },
        { SingleRecommendationStatusButton, RecommendationStatusButtonFallback },
        { SingleRecommendationHistoryHeader, RecommendationHistoryHeaderFallback },
        { SingleRecommendationStatusHint, EmptyFallback },
        { SingleRecommendationCommentLabel, RecommendationCommentLabelFallback },
        { SingleRecommendationCommentHint, RecommendationCommentHintFallback },
        { SingleRecommendationHistoryInitial, RecommendationHistoryInitialFallback },
        { SingleRecommendationHistoryAnswer, EmptyFallback },
        { SingleRecommendationHistoryChange, RecommendationHistoryChangeFallback },
        { SingleRecommendationHistoryReason, EmptyFallback },
        { ViewAnswersHeader, ViewAnswersHeaderFallback },
        { ViewAnswersIntroText, EmptyFallback },
        { ViewAnswersIntroMore, EmptyFallback },
        { ViewAnswersBackToRecs, ViewAnswersBackToRecsFallback },
        { ContinueHeader, ContinueHeaderFallback },
        { ContinueIntroText, EmptyFallback },
        { ContinueAnswersHeader, ContinueAnswersHeaderFallback },
        { ContinueInfoText, EmptyFallback },
        { ContinueInfoMore, EmptyFallback },
        { ContinueContinueButton, ContinueContinueButtonFallback },
        { ContinueRestartButton, ContinueRestartButtonFallback },
        { CheckAnswersChangeLink, CheckAnswersChangeLinkFallback },
        { CheckAnswersSubmitButton, CheckAnswersSubmitButtonFallback },
        { CateogoryPrintSectionNotStarted, PrintSectionIncompleteFallback },
        { CategoryPrintSectionInProgress, PrintSectionIncompleteFallback },
        { CategoryPrintSectionCompleted, EmptyFallback },
        { GroupsSelectHeader, GroupsSelectHeaderFallback },
        { GroupsSelectRecommendationCount, EmptyFallback },
        { GroupsSelectContactUs, EmptyFallback },
        { msmTopicStartHeading, MsmTopicStartHeadingFallback },
        { msmTopicStartText1, MsmTopicStartText1Fallback },
        { msmTopicStartText2, MsmTopicStartText2Fallback },
        { GroupsSelectSchoolsToAssessBackLink, GroupsSelectSchoolsToAssessBackLinkFallback },
        { GroupsSelectSchoolsToAssessHeader, GroupsSelectSchoolsToAssessHeaderFallback },
        { GroupsSelectSchoolsToAssessHint, EmptyFallback },
        { GroupsSelectSchoolsToAssessSchoolHint, EmptyFallback },
        { GroupsSelectSchoolsToAssessCheckboxAll, GroupsSelectSchoolsToAssessCheckboxAllFallback },
        { GroupsSelectSchoolsToAssessWarning, GroupsSelectSchoolsToAssessWarningFallback },
        { GroupsSelectSchoolsToAssessContinue, GroupsSelectSchoolsToAssessContinueFallback },
        { GroupsSelectSchoolsToAssessNoSelectionError, GroupsSelectSchoolsToAssessNoSelectionErrorFallback },
        { GroupsSelectSchoolsToAssessConflictError, GroupsSelectSchoolsToAssessConflictErrorFallback },
    };

    internal static class VariableNames
    {
        public const string Standard = "standard";
        public const string Topic = "topic";
        public const string DateUpdated = "dateUpdated";
        public const string DateCompleted = "dateCompleted";
        public const string Position = "position";
        public const string TotalRecsForTopic = "totalRecsForTopic";
        public const string RecStatus = "recStatus";
        public const string Establishment = "establishment";
        public const string Answer = "answer";
        public const string Count = "count";
        public const string Total = "total";
        public const string ContactLink = "contactLink";
        public const string EstablishmentName = "establishmentName";
        public const string Category = "category";
        public const string SchoolCount = "schoolCount";
        public const string Link = "link";
    }

    public static readonly IReadOnlyDictionary<string, List<string>> Variables = new Dictionary<
        string,
        List<string>
    >
    {
        { LandingPagePrintLink, [VariableNames.Standard] },
        { LandingPageInsetIntroNotStarted, [VariableNames.Standard] },
        { LandingPageInsetIntroContinue, [VariableNames.Standard] },
        { LandingPageTopicLinkNotStarted, [VariableNames.Topic] },
        { LandingPageTopicIntroContinue, [VariableNames.DateUpdated, VariableNames.EstablishmentName] },
        { LandingPageTopicLinkContinue, [VariableNames.Topic] },
        { LandingPageTopicIntroCompleted, [VariableNames.Topic, VariableNames.DateCompleted, VariableNames.EstablishmentName] },
        { LandingPageTopicLinkCompleted, [VariableNames.Topic] },
        { LandingPageSuccessPanelHeader, [VariableNames.Topic] },
        { SingleRecommendationPrintAllLink, [VariableNames.Topic] },
        { SingleRecommendationSuccessHeader, [VariableNames.RecStatus] },
        { SingleRecommendationPosition, [VariableNames.Position, VariableNames.TotalRecsForTopic] },
        {
            SingleRecommendationHistoryInitial,
            [VariableNames.RecStatus, VariableNames.Establishment]
        },
        { SingleRecommendationHistoryAnswer, [VariableNames.Answer] },
        { SingleRecommendationHistoryChange, [VariableNames.RecStatus] },
        { SingleRecommendationHistoryReason, [VariableNames.RecStatus] },
        { ViewAnswersIntroText, [VariableNames.DateCompleted] },
        { ContinueIntroText, [VariableNames.DateUpdated] },
        { CategoryPrintSectionInProgress, [VariableNames.DateUpdated] },
        { CategoryPrintSectionCompleted, [VariableNames.Topic, VariableNames.DateCompleted] },
        { GroupsSelectRecommendationCount, [VariableNames.Count, VariableNames.Total] },
        { GroupsSelectContactUs, [VariableNames.ContactLink] },
        { SchoolSummarySuccessBody, [VariableNames.Topic] },
        { SchoolSummaryRecLink, [VariableNames.Topic] },
        { SummaryRecBody, [VariableNames.Category] },
        { MatSummarySuccessBody, [VariableNames.Topic, VariableNames.SchoolCount] },
        { MatSummaryRecLinkIntro, [VariableNames.Topic] },
        { MatSummarySubmitAnotherBody, [VariableNames.Link] },
        { GroupsSelectSchoolsToAssessBackLink, [VariableNames.Topic] },
        { GroupsSelectSchoolsToAssessSchoolHint, [VariableNames.DateUpdated] }
    };
}
