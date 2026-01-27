using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Data.Contentful.Persistence;

namespace Dfe.PlanTech.Data.Contentful.UnitTests.Persistence
{
    public class EntityResolverTests
    {
        [Fact]
        public void Should_Find_Types()
        {
            var entityResolver = new EntryResolver();

            Assert.NotEmpty(entityResolver.Types);
        }

        [Fact]
        public void ContentfulContentId_KnownContentfulId_ReturnMatchingCsharpType()
        {
            // Arrange
            var expectedType = typeof(ComponentAccordionEntry);
            var contentfulTypeId = "CSAccordion";

            var entityResolver = new EntryResolver();

            // Act
            var foundType = entityResolver.Resolve(contentfulTypeId);

            // Assert
            Assert.Equal(expectedType, foundType);
        }

        [Fact]
        public void ContentfulContentId_UnknownContentfulId_ReturnMissingComponentEntry()
        {
            // Arrange
            var expectedType = typeof(MissingComponentEntry);
            var contentfulTypeId = "not a real type";

            var entityResolver = new EntryResolver();

            // Act
            var foundType = entityResolver.Resolve(contentfulTypeId);

            // Assert
            Assert.Equal(expectedType, foundType);
        }

        [Theory]
        [InlineData(ContentfulContentTypeConstants.CAndSLinkContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.ComponentAccordionContentfulContentTypeId)]
        [InlineData(
            ContentfulContentTypeConstants.ComponentAccordionSectionContentfulContentTypeId
        )]
        [InlineData(ContentfulContentTypeConstants.ComponentAttachmentContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.ComponentButtonContentfulContentTypeId)]
        [InlineData(
            ContentfulContentTypeConstants.ComponentButtonWithEntryReferenceContentfulContentTypeId
        )]
        [InlineData(ContentfulContentTypeConstants.ComponentButtonWithLinkContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.ComponentCardContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.ComponentCsHeadingContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.ComponentDropDownContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.ComponentDynamicContentContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.ComponentGridContainerContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.ComponentHeaderContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.ComponentHeroContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.ComponentInsetTextContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.ComponentJumpLinkContentfulContentTypeId)]
        [InlineData(
            ContentfulContentTypeConstants.ComponentNotificationBannerContentfulContentTypeId
        )]
        [InlineData(ContentfulContentTypeConstants.ComponentTextBodyContentfulContentTypeId)]
        [InlineData(
            ContentfulContentTypeConstants.ComponentTextBodyWithMaturityContentfulContentTypeId
        )]
        [InlineData(ContentfulContentTypeConstants.ComponentTitleContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.ComponentWarningContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.ContentSupportPageContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.CsBodyTextContentfulContentTypeId)]
        // [InlineData(ContentfulContentTypeConstants.MissingComponentContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.NavigationLinkContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.PageContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.PageRecommendationContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.QuestionnaireAnswerContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.QuestionnaireCategoryContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.QuestionnaireQuestionContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.QuestionnaireSectionContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.RecommendationChunkContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.RecommendationPageContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.RichTextContentContentfulContentTypeId)]
        [InlineData(ContentfulContentTypeConstants.RichTextContentDataContentfulContentTypeId)]
        [InlineData(
            ContentfulContentTypeConstants.RichTextContentSupportDataContentfulContentTypeId
        )]
        [InlineData(ContentfulContentTypeConstants.RichTextMarkContentfulContentTypeId)]
        public void ContentfulContentId_KnownContentfulId_ReturnsMatchingCsharpType_ForAllMappings(
            string contentfulTypeId
        )
        {
            // Arrange
            var entityResolver = new EntryResolver();

            // Act
            var foundType = entityResolver.Resolve(contentfulTypeId);

            // Assert
            Assert.NotEqual(typeof(MissingComponentEntry), foundType);
        }
    }

    // TODO: Fetch all content models from Contentful, and validate that we are able to map them to a C# class.
}
