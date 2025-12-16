using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.Constants;

[ExcludeFromCodeCoverage]
public static class ContentfulContentTypeConstants
{
    // Contentful ID to C# class type mappings
    public static Dictionary<string, Type> ContentTypeToEntryClassTypeMap =>
        EntryClassToContentTypeMapBasis.Keys.ToDictionary(key => EntryClassToContentTypeMapBasis[key].ToLower(), key => key);

    // C# class name to Contentful ID mappings
    public static Dictionary<string, string> EntryClassToContentTypeMap =>
        EntryClassToContentTypeMapBasis.Keys.ToDictionary(key => key.Name, key => EntryClassToContentTypeMapBasis[key]);


    // Internal mappings between C# and Contentful content type ID's
    private static readonly Dictionary<Type, string> EntryClassToContentTypeMapBasis = new()
        {
            { typeof(CAndSLinkEntry), CAndSLinkContentfulContentTypeId },
            { typeof(ComponentAccordionEntry), ComponentAccordionContentfulContentTypeId },
            { typeof(ComponentAccordionSectionEntry), ComponentAccordionSectionContentfulContentTypeId },
            { typeof(ComponentAttachmentEntry), ComponentAttachmentContentfulContentTypeId },
            { typeof(ComponentButtonEntry), ComponentButtonContentfulContentTypeId },
            { typeof(ComponentButtonWithEntryReferenceEntry), ComponentButtonWithEntryReferenceContentfulContentTypeId },
            { typeof(ComponentButtonWithLinkEntry), ComponentButtonWithLinkContentfulContentTypeId },
            { typeof(ComponentCardEntry), ComponentCardContentfulContentTypeId },
            { typeof(ComponentCsHeadingEntry), ComponentCsHeadingContentfulContentTypeId},
            { typeof(ComponentDropDownEntry), ComponentDropDownContentfulContentTypeId },
            { typeof(ComponentDynamicContentEntry), ComponentDynamicContentContentfulContentTypeId },
            { typeof(ComponentGridContainerEntry), ComponentGridContainerContentfulContentTypeId },
            { typeof(ComponentHeaderEntry), ComponentHeaderContentfulContentTypeId },
            { typeof(ComponentHeroEntry), ComponentHeroContentfulContentTypeId },
            { typeof(ComponentInsetTextEntry), ComponentInsetTextContentfulContentTypeId },
            { typeof(ComponentJumpLinkEntry), ComponentJumpLinkContentfulContentTypeId },
            { typeof(ComponentNotificationBannerEntry), ComponentNotificationBannerContentfulContentTypeId },
            { typeof(ComponentTextBodyEntry), ComponentTextBodyContentfulContentTypeId },
            { typeof(ComponentTextBodyWithMaturityEntry), ComponentTextBodyWithMaturityContentfulContentTypeId },
            { typeof(ComponentTitleEntry), ComponentTitleContentfulContentTypeId },
            { typeof(ComponentWarningEntry), ComponentWarningContentfulContentTypeId },
            { typeof(ContentSupportPageEntry), ContentSupportPageContentfulContentTypeId },
            { typeof(CsBodyTextEntry), CsBodyTextContentfulContentTypeId },
            { typeof(MissingComponentEntry), MissingComponentContentfulContentTypeId },
            { typeof(NavigationLinkEntry), NavigationLinkContentfulContentTypeId },
            { typeof(PageEntry), PageContentfulContentTypeId },
            { typeof(PageRecommendationEntry), PageRecommendationContentfulContentTypeId },
            { typeof(QuestionnaireAnswerEntry), QuestionnaireAnswerContentfulContentTypeId },
            { typeof(QuestionnaireCategoryEntry), QuestionnaireCategoryContentfulContentTypeId },
            { typeof(QuestionnaireQuestionEntry), QuestionnaireQuestionContentfulContentTypeId },
            { typeof(QuestionnaireSectionEntry), QuestionnaireSectionContentfulContentTypeId },
            { typeof(RecommendationChunkEntry), RecommendationChunkContentfulContentTypeId },
            { typeof(RecommendationPageEntry), RecommendationPageContentfulContentTypeId },
            { typeof(RichTextContentDataEntry), RichTextContentDataContentfulContentTypeId },
            { typeof(RichTextContentField), RichTextContentContentfulContentTypeId },
            { typeof(RichTextContentSupportDataField), RichTextContentSupportDataContentfulContentTypeId },
            { typeof(RichTextMarkField), RichTextMarkContentfulContentTypeId },
        };

    // Contentful content type IDs are managed in Contentful.
    // These hardcoded strings MUST match the JSON IDs as provided by Contentful's API.
    public const string CAndSLinkContentfulContentTypeId = "csLink";
    public const string ComponentAccordionContentfulContentTypeId = "CSAccordion";
    public const string ComponentAccordionSectionContentfulContentTypeId = "accordionSection";
    public const string ComponentAttachmentContentfulContentTypeId = "Attachment";
    public const string ComponentButtonContentfulContentTypeId = "button";
    public const string ComponentButtonWithEntryReferenceContentfulContentTypeId = "buttonWithEntryReference";
    public const string ComponentButtonWithLinkContentfulContentTypeId = "buttonWithLink";
    public const string ComponentCardContentfulContentTypeId = "csCard";
    public const string ComponentCsHeadingContentfulContentTypeId = "csHeading";
    public const string ComponentDropDownContentfulContentTypeId = "componentDropDown";
    public const string ComponentDynamicContentContentfulContentTypeId = "dynamicContent";
    public const string ComponentGridContainerContentfulContentTypeId = "gridContainer";
    public const string ComponentHeaderContentfulContentTypeId = "header";
    public const string ComponentHeroContentfulContentTypeId = "componentHero";
    public const string ComponentInsetTextContentfulContentTypeId = "insetText";
    public const string ComponentJumpLinkContentfulContentTypeId = "csJumpLinkComponent";
    public const string ComponentNotificationBannerContentfulContentTypeId = "notificationBanner";
    public const string ComponentTextBodyContentfulContentTypeId = "textBody";
    public const string ComponentTextBodyWithMaturityContentfulContentTypeId = "componentTextBodyWithMaturity";
    public const string ComponentTitleContentfulContentTypeId = "title";
    public const string ComponentWarningContentfulContentTypeId = "warningComponent";
    public const string ContentSupportPageContentfulContentTypeId = "contentSupportPage";
    public const string CsBodyTextContentfulContentTypeId = "csBodyText";
    public const string MissingComponentContentfulContentTypeId = "missingComponent";
    public const string NavigationLinkContentfulContentTypeId = "navigationLink";
    public const string PageContentfulContentTypeId = "page";
    public const string PageRecommendationContentfulContentTypeId = "pageRecommendation";
    public const string QuestionnaireAnswerContentfulContentTypeId = "answer";
    public const string QuestionnaireCategoryContentfulContentTypeId = "category";
    public const string QuestionnaireQuestionContentfulContentTypeId = "Question";
    public const string QuestionnaireSectionContentfulContentTypeId = "section";
    public const string RecommendationChunkContentfulContentTypeId = "recommendationChunk";
    public const string RecommendationPageContentfulContentTypeId = "recommendationPage";
    public const string RichTextContentContentfulContentTypeId = "richTextContent";
    public const string RichTextContentDataContentfulContentTypeId = "richTextContentData";
    public const string RichTextContentSupportDataContentfulContentTypeId = "richTextContentSupportData";
    public const string RichTextMarkContentfulContentTypeId = "richTextMark";
}
