using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.Constants;

public static class ContentTypeConstants
{
    public static readonly Dictionary<string, string> EntryClassToContentTypeMap = new()
        {
            { nameof(CAndSLinkEntry), CAndSLinkContentTypeId },
            { nameof(ComponentAccordionEntry), ComponentAccordionContentTypeId },
            { nameof(ComponentAccordionSectionEntry), ComponentAccordionSectionContentTypeId },
            { nameof(ComponentAttachmentEntry), ComponentAttachmentContentTypeId },
            { nameof(ComponentButtonEntry), ComponentButtonContentTypeId },
            { nameof(ComponentButtonWithEntryReferenceEntry), ComponentButtonWithEntryReferenceContentTypeId },
            { nameof(ComponentButtonWithLinkEntry), ComponentButtonWithLinkContentTypeId },
            { nameof(ComponentCardEntry), ComponentCardContentTypeId },
            { nameof(ComponentDropDownEntry), ComponentDropDownContentTypeId },
            { nameof(ComponentCsHeadingEntry), ComponentCsHeadingContentTypeId},
            { nameof(ComponentDynamicContentEntry), ComponentDynamicContentContentTypeId },
            { nameof(ComponentGridContainerEntry), ComponentGridContainerContentTypeId },
            { nameof(ComponentHeaderEntry), ComponentHeaderContentTypeId },
            { nameof(ComponentHeroEntry), ComponentHeroContentTypeId },
            { nameof(ComponentInsetTextEntry), ComponentInsetTextContentTypeId },
            { nameof(ComponentJumpLinkEntry), ComponentJumpLinkContentTypeId },
            { nameof(ComponentNotificationBannerEntry), ComponentNotificationBannerContentTypeId },
            { nameof(ComponentTextBodyEntry), ComponentTextBodyContentTypeId },
            { nameof(ComponentTextBodyWithMaturityEntry), ComponentTextBodyWithMaturityContentTypeId },
            { nameof(ComponentTitleEntry), ComponentTitleContentTypeId },
            { nameof(ComponentWarningEntry), ComponentWarningContentTypeId },
            { nameof(ContentSupportPageEntry), ContentSupportPageContentTypeId },
            { nameof(CsBodyTextEntry), CsBodyTextContentTypeId },
            { nameof(NavigationLinkEntry), NavigationLinkContentTypeId },
            { nameof(PageEntry), PageContentTypeId },
            { nameof(PageRecommendationEntry), PageRecommendationContentTypeId },
            { nameof(QuestionnaireAnswerEntry), QuestionnaireAnswerContentTypeId },
            { nameof(QuestionnaireCategoryEntry), QuestionnaireCategoryContentTypeId },
            { nameof(QuestionnaireQuestionEntry), QuestionnaireQuestionContentTypeId },
            { nameof(QuestionnaireSectionEntry), QuestionnaireSectionContentTypeId },
            { nameof(RecommendationChunkEntry), RecommendationChunkContentTypeId },
            { nameof(RecommendationIntroEntry), RecommendationIntroContentTypeId },
            { nameof(RecommendationPageEntry), RecommendationPageContentTypeId },
            { nameof(RecommendationSectionEntry), RecommendationSectionContentTypeId },
            { nameof(SubtopicRecommendationEntry), SubtopicRecommendationContentTypeId },
        };

    public const string CAndSLinkContentTypeId = "csLink";
    public const string ComponentAccordionContentTypeId = "CSAccordion";
    public const string ComponentAccordionSectionContentTypeId = "AccordionSection";
    public const string ComponentAttachmentContentTypeId = "Attachment";
    public const string ComponentButtonContentTypeId = "button";
    public const string ComponentButtonWithEntryReferenceContentTypeId = "buttonWithEntryReference";
    public const string ComponentButtonWithLinkContentTypeId = "buttonWithLink";
    public const string ComponentCardContentTypeId = "csCard";
    public const string ComponentCsHeadingContentTypeId = "CSHeading";
    public const string ComponentDropDownContentTypeId = "componentDropDown";
    public const string ComponentDynamicContentContentTypeId = "dynamicContent";
    public const string ComponentGridContainerContentTypeId = "GridContainer";
    public const string ComponentHeaderContentTypeId = "header";
    public const string ComponentHeroContentTypeId = "componentHero";
    public const string ComponentInsetTextContentTypeId = "insetText";
    public const string ComponentJumpLinkContentTypeId = "csJumpLinkComponent";
    public const string ComponentNotificationBannerContentTypeId = "notificationBanner";
    public const string ComponentTextBodyContentTypeId = "textBody";
    public const string ComponentTextBodyWithMaturityContentTypeId = "componentTextBodyWithMaturity";
    public const string ComponentTitleContentTypeId = "title";
    public const string ComponentWarningContentTypeId = "warningComponent";
    public const string ContentSupportPageContentTypeId = "ContentSupportPage";
    public const string CsBodyTextContentTypeId = "CSBodyText";
    public const string NavigationLinkContentTypeId = "navigationLink";
    public const string PageContentTypeId = "page";
    public const string PageRecommendationContentTypeId = "pageRecommendation";
    public const string QuestionnaireAnswerContentTypeId = "answer";
    public const string QuestionnaireCategoryContentTypeId = "category";
    public const string QuestionnaireQuestionContentTypeId = "question";
    public const string QuestionnaireSectionContentTypeId = "section";
    public const string RecommendationChunkContentTypeId = "recommendationChunk";
    public const string RecommendationIntroContentTypeId = "recommendationIntro";
    public const string RecommendationPageContentTypeId = "recommendationPage";
    public const string RecommendationSectionContentTypeId = "recommendationSection";
    public const string SubtopicRecommendationContentTypeId = "subtopicRecommendation";
}
