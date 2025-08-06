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
            { nameof(RichTextContentField), RichTextContentContentTypeId },
            { nameof(RichTextContentDataEntry), RichTextContentDataContentTypeId },
            { nameof(RichTextContentSupportDataField), RichTextContentSupportDataContentTypeId },
            { nameof(RichTextMarkField), RichTextMarkContentTypeId },
            { nameof(SubtopicRecommendationEntry), SubtopicRecommendationContentTypeId }
        };

    public static readonly Dictionary<string, Type> ContentTypeToEntryClassTypeMap = new()
        {
            { CAndSLinkContentTypeId.ToLower(), typeof(CAndSLinkEntry) },
            { ComponentAccordionContentTypeId.ToLower(), typeof(ComponentAccordionEntry) },
            { ComponentAccordionSectionContentTypeId.ToLower(), typeof(ComponentAccordionSectionEntry) },
            { ComponentAttachmentContentTypeId.ToLower(), typeof(ComponentAttachmentEntry) },
            { ComponentButtonContentTypeId.ToLower(), typeof(ComponentButtonEntry) },
            { ComponentButtonWithEntryReferenceContentTypeId.ToLower(), typeof(ComponentButtonWithEntryReferenceEntry) },
            { ComponentButtonWithLinkContentTypeId.ToLower(), typeof(ComponentButtonWithLinkEntry) },
            { ComponentCardContentTypeId.ToLower(), typeof(ComponentCardEntry) },
            { ComponentDropDownContentTypeId.ToLower(), typeof(ComponentDropDownEntry) },
            { ComponentCsHeadingContentTypeId.ToLower(), typeof(ComponentCsHeadingEntry) },
            { ComponentDynamicContentContentTypeId.ToLower(), typeof(ComponentDynamicContentEntry) },
            { ComponentGridContainerContentTypeId.ToLower(), typeof(ComponentGridContainerEntry) },
            { ComponentHeaderContentTypeId.ToLower(), typeof(ComponentHeaderEntry) },
            { ComponentHeroContentTypeId.ToLower(), typeof(ComponentHeroEntry) },
            { ComponentInsetTextContentTypeId.ToLower(), typeof(ComponentInsetTextEntry) },
            { ComponentJumpLinkContentTypeId.ToLower(), typeof(ComponentJumpLinkEntry) },
            { ComponentNotificationBannerContentTypeId.ToLower(), typeof(ComponentNotificationBannerEntry) },
            { ComponentTextBodyContentTypeId.ToLower(), typeof(ComponentTextBodyEntry) },
            { ComponentTextBodyWithMaturityContentTypeId.ToLower(), typeof(ComponentTextBodyWithMaturityEntry) },
            { ComponentTitleContentTypeId.ToLower(), typeof(ComponentTitleEntry) },
            { ComponentWarningContentTypeId.ToLower(), typeof(ComponentWarningEntry) },
            { ContentSupportPageContentTypeId.ToLower(), typeof(ContentSupportPageEntry) },
            { CsBodyTextContentTypeId.ToLower(), typeof(CsBodyTextEntry) },
            { MissingComponentContentTypeId.ToLower(), typeof(MissingComponentEntry) },
            { NavigationLinkContentTypeId.ToLower(), typeof(NavigationLinkEntry) },
            { PageContentTypeId.ToLower(), typeof(PageEntry) },
            { PageRecommendationContentTypeId.ToLower(), typeof(PageRecommendationEntry) },
            { QuestionnaireAnswerContentTypeId.ToLower(), typeof(QuestionnaireAnswerEntry) },
            { QuestionnaireCategoryContentTypeId.ToLower(), typeof(QuestionnaireCategoryEntry) },
            { QuestionnaireQuestionContentTypeId.ToLower(), typeof(QuestionnaireQuestionEntry) },
            { QuestionnaireSectionContentTypeId.ToLower(), typeof(QuestionnaireSectionEntry) },
            { RecommendationChunkContentTypeId.ToLower(), typeof(RecommendationChunkEntry) },
            { RecommendationIntroContentTypeId.ToLower(), typeof(RecommendationIntroEntry) },
            { RecommendationPageContentTypeId.ToLower(), typeof(RecommendationPageEntry) },
            { RecommendationSectionContentTypeId.ToLower(), typeof(RecommendationSectionEntry) },
            { RichTextContentContentTypeId.ToLower(), typeof(RichTextContentField) },
            { RichTextContentDataContentTypeId.ToLower(), typeof(RichTextContentDataEntry) },
            { RichTextContentSupportDataContentTypeId.ToLower(), typeof(RichTextContentSupportDataField) },
            { RichTextMarkContentTypeId.ToLower(), typeof(RichTextMarkField) },
            { SubtopicRecommendationContentTypeId.ToLower(), typeof(SubtopicRecommendationEntry) }
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
    public const string ComponentTextBodyContentTypeId = "TextBody";
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
}
