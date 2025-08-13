using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.Constants;

public static class ContentTypeConstants
{
    public static Dictionary<string, Type> ContentTypeToEntryClassTypeMap =>
        EntryClassToContentTypeMapBasis.Keys.ToDictionary(key => EntryClassToContentTypeMapBasis[key].ToLower(), key => key);

    public static Dictionary<string, string> EntryClassToContentTypeMap =>
        EntryClassToContentTypeMapBasis.Keys.ToDictionary(key => key.Name, key => EntryClassToContentTypeMapBasis[key]);

    private static readonly Dictionary<Type, string> EntryClassToContentTypeMapBasis = new()
        {
            { typeof(CAndSLinkEntry), CAndSLinkContentTypeId },
            { typeof(ComponentAccordionEntry), ComponentAccordionContentTypeId },
            { typeof(ComponentAccordionSectionEntry), ComponentAccordionSectionContentTypeId },
            { typeof(ComponentAttachmentEntry), ComponentAttachmentContentTypeId },
            { typeof(ComponentButtonEntry), ComponentButtonContentTypeId },
            { typeof(ComponentButtonWithEntryReferenceEntry), ComponentButtonWithEntryReferenceContentTypeId },
            { typeof(ComponentButtonWithLinkEntry), ComponentButtonWithLinkContentTypeId },
            { typeof(ComponentCardEntry), ComponentCardContentTypeId },
            { typeof(ComponentDropDownEntry), ComponentDropDownContentTypeId },
            { typeof(ComponentCsHeadingEntry), ComponentCsHeadingContentTypeId},
            { typeof(ComponentDynamicContentEntry), ComponentDynamicContentContentTypeId },
            { typeof(ComponentGridContainerEntry), ComponentGridContainerContentTypeId },
            { typeof(ComponentHeaderEntry), ComponentHeaderContentTypeId },
            { typeof(ComponentHeroEntry), ComponentHeroContentTypeId },
            { typeof(ComponentInsetTextEntry), ComponentInsetTextContentTypeId },
            { typeof(ComponentJumpLinkEntry), ComponentJumpLinkContentTypeId },
            { typeof(ComponentNotificationBannerEntry), ComponentNotificationBannerContentTypeId },
            { typeof(ComponentTextBodyEntry), ComponentTextBodyContentTypeId },
            { typeof(ComponentTextBodyWithMaturityEntry), ComponentTextBodyWithMaturityContentTypeId },
            { typeof(ComponentTitleEntry), ComponentTitleContentTypeId },
            { typeof(ComponentWarningEntry), ComponentWarningContentTypeId },
            { typeof(ContentSupportPageEntry), ContentSupportPageContentTypeId },
            { typeof(CsBodyTextEntry), CsBodyTextContentTypeId },
            { typeof(MissingComponentEntry), MissingComponentContentTypeId },
            { typeof(NavigationLinkEntry), NavigationLinkContentTypeId },
            { typeof(PageEntry), PageContentTypeId },
            { typeof(PageRecommendationEntry), PageRecommendationContentTypeId },
            { typeof(QuestionnaireAnswerEntry), QuestionnaireAnswerContentTypeId },
            { typeof(QuestionnaireCategoryEntry), QuestionnaireCategoryContentTypeId },
            { typeof(QuestionnaireQuestionEntry), QuestionnaireQuestionContentTypeId },
            { typeof(QuestionnaireSectionEntry), QuestionnaireSectionContentTypeId },
            { typeof(RecommendationChunkEntry), RecommendationChunkContentTypeId },
            { typeof(RecommendationIntroEntry), RecommendationIntroContentTypeId },
            { typeof(RecommendationPageEntry), RecommendationPageContentTypeId },
            { typeof(RecommendationSectionEntry), RecommendationSectionContentTypeId },
            { typeof(RichTextContentField), RichTextContentContentTypeId },
            { typeof(RichTextContentDataEntry), RichTextContentDataContentTypeId },
            { typeof(RichTextContentSupportDataField), RichTextContentSupportDataContentTypeId },
            { typeof(RichTextMarkField), RichTextMarkContentTypeId },
            { typeof(SubtopicRecommendationEntry), SubtopicRecommendationContentTypeId }
        };

    public const string CAndSLinkContentTypeId = "csLink";
    public const string ComponentAccordionContentTypeId = "csAccordion";
    public const string ComponentAccordionSectionContentTypeId = "accordionSection";
    public const string ComponentAttachmentContentTypeId = "attachment";
    public const string ComponentButtonContentTypeId = "button";
    public const string ComponentButtonWithEntryReferenceContentTypeId = "buttonWithEntryReference";
    public const string ComponentButtonWithLinkContentTypeId = "buttonWithLink";
    public const string ComponentCardContentTypeId = "csCard";
    public const string ComponentCsHeadingContentTypeId = "csHeading";
    public const string ComponentDropDownContentTypeId = "componentDropDown";
    public const string ComponentDynamicContentContentTypeId = "dynamicContent";
    public const string ComponentGridContainerContentTypeId = "gridContainer";
    public const string ComponentHeaderContentTypeId = "header";
    public const string ComponentHeroContentTypeId = "componentHero";
    public const string ComponentInsetTextContentTypeId = "insetText";
    public const string ComponentJumpLinkContentTypeId = "csJumpLinkComponent";
    public const string ComponentNotificationBannerContentTypeId = "notificationBanner";
    public const string ComponentTextBodyContentTypeId = "textBody";
    public const string ComponentTextBodyWithMaturityContentTypeId = "componentTextBodyWithMaturity";
    public const string ComponentTitleContentTypeId = "title";
    public const string ComponentWarningContentTypeId = "warningComponent";
    public const string ContentSupportPageContentTypeId = "contentSupportPage";
    public const string CsBodyTextContentTypeId = "csBodyText";
    public const string MissingComponentContentTypeId = "missingComponent";
    public const string NavigationLinkContentTypeId = "navigationLink";
    public const string PageContentTypeId = "page";
    public const string PageRecommendationContentTypeId = "pageRecommendation";
    public const string QuestionnaireAnswerContentTypeId = "answer";
    public const string QuestionnaireCategoryContentTypeId = "category";
    public const string QuestionnaireQuestionContentTypeId = "Question";
    public const string QuestionnaireSectionContentTypeId = "section";
    public const string RecommendationChunkContentTypeId = "recommendationChunk";
    public const string RecommendationIntroContentTypeId = "recommendationIntro";
    public const string RecommendationPageContentTypeId = "recommendationPage";
    public const string RecommendationSectionContentTypeId = "recommendationSection";
    public const string RichTextContentContentTypeId = "richTextContent";
    public const string RichTextContentDataContentTypeId = "richTextContentData";
    public const string RichTextContentSupportDataContentTypeId = "richTextContentSupportData";
    public const string RichTextMarkContentTypeId = "richTextMark";
    public const string SubtopicRecommendationContentTypeId = "subtopicRecommendation";
}
