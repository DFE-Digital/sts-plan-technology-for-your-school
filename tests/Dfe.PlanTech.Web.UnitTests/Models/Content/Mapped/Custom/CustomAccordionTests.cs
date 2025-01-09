using Contentful.Core.Models;
using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Custom;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Standard;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Types;
using Dfe.PlanTech.Web.Configuration;
using FluentAssertions;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Models.Content.Mapped.Custom;

public class CustomAccordionTests
{
    private static ModelMapper GetService() => new ModelMapper(new SupportedAssetTypes());

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
                    InternalName = "content name",
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

        var expectedBody = new RichTextContentItem
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
