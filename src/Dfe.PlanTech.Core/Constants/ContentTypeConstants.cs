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
            { nameof(MissingComponentEntry), MissingComponentContentTypeId },
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
            { nameof(RichTextContentEntry), RichTextContentContentTypeId },
            { nameof(RichTextContentDataEntry), RichTextContentDataContentTypeId },
            { nameof(RichTextContentSupportDataEntry), RichTextContentSupportDataContentTypeId },
            { nameof(RichTextMarkEntry), RichTextMarkContentTypeId },
            { nameof(SubtopicRecommendationEntry), SubtopicRecommendationContentTypeId },
            { nameof(TextBodyEntry), TextBodyContentTypeId }
        };

    public const string CAndSLinkContentTypeId = "csLink";
    public const string ComponentAccordionContentTypeId = "CSAccordion";
    public const string ComponentAccordionSectionContentTypeId = "AccordionSection";
    public const string ComponentAttachmentContentTypeId = "Attachment";
    public const string ComponentButtonContentTypeId = "Button";
    public const string ComponentButtonWithEntryReferenceContentTypeId = "ButtonWithEntryReference";
    public const string ComponentButtonWithLinkContentTypeId = "ButtonWithLink";
    public const string ComponentCardContentTypeId = "csCard";
    public const string ComponentCsHeadingContentTypeId = "CSHeading";
    public const string ComponentDropDownContentTypeId = "componentDropDown";
    public const string ComponentDynamicContentContentTypeId = "dynamicContent";
    public const string ComponentGridContainerContentTypeId = "GridContainer";
    public const string ComponentHeaderContentTypeId = "Header";
    public const string ComponentHeroContentTypeId = "componentHero";
    public const string ComponentInsetTextContentTypeId = "insetText";
    public const string ComponentJumpLinkContentTypeId = "csJumpLinkComponent";
    public const string ComponentNotificationBannerContentTypeId = "NotificationBanner";
    public const string ComponentTextBodyContentTypeId = "textBody";
    public const string ComponentTextBodyWithMaturityContentTypeId = "componentTextBodyWithMaturity";
    public const string ComponentTitleContentTypeId = "title";
    public const string ComponentWarningContentTypeId = "WarningComponent";
    public const string ContentSupportPageContentTypeId = "ContentSupportPage";
    public const string CsBodyTextContentTypeId = "CSBodyText";
    public const string MissingComponentContentTypeId = "MissingComponent";
    public const string NavigationLinkContentTypeId = "navigationLink";
    public const string PageContentTypeId = "page";
    public const string PageRecommendationContentTypeId = "pageRecommendation";
    public const string QuestionnaireAnswerContentTypeId = "answer";
    public const string QuestionnaireCategoryContentTypeId = "Category";
    public const string QuestionnaireQuestionContentTypeId = "Question";
    public const string QuestionnaireSectionContentTypeId = "Section";
    public const string RecommendationChunkContentTypeId = "RecommendationChunk";
    public const string RecommendationIntroContentTypeId = "RecommendationIntro";
    public const string RecommendationPageContentTypeId = "RecommendationPage";
    public const string RecommendationSectionContentTypeId = "RecommendationSection";
    public const string RichTextContentContentTypeId = "RichTextContent";
    public const string RichTextContentDataContentTypeId = "RichTextContentData";
    public const string RichTextContentSupportDataContentTypeId = "RichTextContentSupportData";
    public const string RichTextMarkContentTypeId = "RichTextMark";
    public const string SubtopicRecommendationContentTypeId = "subtopicRecommendation";
    public const string TextBodyContentTypeId = "TextBody";
}
