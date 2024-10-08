using Contentful.Core.Models;
using Dfe.ContentSupport.Web.Common;
using Dfe.ContentSupport.Web.Configuration;
using Dfe.ContentSupport.Web.Models;
using Dfe.ContentSupport.Web.Models.Mapped;
using Dfe.ContentSupport.Web.Models.Mapped.Custom;
using Dfe.ContentSupport.Web.Models.Mapped.Standard;
using Dfe.ContentSupport.Web.Models.Mapped.Types;

namespace Dfe.ContentSupport.Web.Tests.Models.Mapped.Custom;

public class CustomAccordionTests
{
    private static IModelMapper GetService() => new ModelMapper(new SupportedAssetTypes());

    private const string ContentId = "CSAccordion";
    private const string InternalName = "Internal Name";
    private const string Title = "Title";
    private const string SummaryLine = "Summary Line";
    private const string ContentInternalName = "Content Internal Name";

    private static ContentItem DummyContentItem() => new()
    {
        NodeType = RichTextTags.EmbeddedEntry,
        Data = new Data
        {
            Target = new Target
            {
                InternalName = InternalName,
                Title = Title,
                SummaryLine = SummaryLine,
                SystemProperties = new SystemProperties
                {
                    ContentType = new Contentful.Core.Models.ContentType
                    {
                        SystemProperties = new SystemProperties
                        {
                            Id = ContentId
                        }
                    }
                },
                Content =
                [
                    new Target(),
                    new Target(),
                    new Target()
                ],
                RichText = new ContentItem
                {
                    InternalName = null,
                    NodeType = "paragraph"
                }
            }
        }
    };


    [Fact]
    public void MapCorrectly()
    {
        var testValue = DummyContentItem();

        var sut = GetService();
        var result = sut.MapContent(testValue);
        result.Should().BeAssignableTo<EmbeddedEntry>();
        var entry = (result as EmbeddedEntry)!;

        entry.NodeType.Should().Be(RichTextNodeType.EmbeddedEntry);
        entry.InternalName.Should().Be(InternalName);
        entry.RichText.Should().NotBeNull();
        entry.CustomComponent.Should().NotBeNull();

        var customComponent = entry.CustomComponent;
        customComponent.Should().BeAssignableTo<CustomAccordion>();
        var accordion = (customComponent as CustomAccordion)!;

        var expectedBody=  new RichTextContentItem
        {
            InternalName = InternalName,
            NodeType = RichTextNodeType.Paragraph,
            Content = []
        };
        
        accordion.Type.Should().Be(CustomComponentType.Accordion);
        accordion.InternalName.Should().Be(InternalName);
        accordion.Title.Should().Be(Title);
        accordion.SummaryLine.Should().Be(SummaryLine);
        accordion.Body.Should().BeEquivalentTo(expectedBody);
        accordion.Accordions.Count.Should().Be(3);
    }
}