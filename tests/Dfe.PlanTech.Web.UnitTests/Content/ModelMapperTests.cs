using Contentful.Core.Models;
using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Web.Configuration;
using Dfe.PlanTech.Web.Content;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Types;
using FluentAssertions;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Content;

public class ModelMapperTests
{
    private static ModelMapper GetService() => new ModelMapper(new SupportedAssetTypes());

    private static ContentItem DummyContentItem() => new()
    {
        NodeType = RichTextTags.Document
    };

    [Fact]
    public void MapToCsPages_Return_Correct_Amount()
    {
        var supportPages = new List<ContentSupportPage>
        {
            new()
            {
               SystemProperties = new SystemProperties()
            },
            new()
            {
                 SystemProperties = new SystemProperties()
            },
            new()
            {
                SystemProperties = new SystemProperties()
            }
        };

        var sut = GetService();

        var result = sut.MapToCsPages(supportPages);

        result.Count.Should().Be(3);
    }

    [Fact]
    public void ConvertEntryToContentItem_IsRichText_Returns_RichTextContentItem()
    {
        var entry = new Entry
        {
            RichText = new ContentItem()
        };

        var sut = GetService();

        var result = sut.ConvertEntryToContentItem(entry);

        result.Should().BeAssignableTo<RichTextContentItem>();
    }

    [Fact]
    public void ConvertEntryToContentItem_NotRichText_NotReturns_RichTextContentItem()
    {
        var entry = new Entry();

        var sut = GetService();

        var result = sut.ConvertEntryToContentItem(entry);

        result.Should().NotBeAssignableTo<RichTextContentItem>();
    }


    [Fact]
    public void Document_Returns_Null()
    {
        var testValue = DummyContentItem();

        var sut = GetService();
        var result = sut.MapContent(testValue);

        result.Should().BeNull();
    }

    [Fact]
    public void BasicComponentReturned()
    {
        var basicComponentTypes = new List<string>
        {
            RichTextTags.Paragraph,
            RichTextTags.UnorderedList,
            RichTextTags.OrderedList,
            RichTextTags.ListItem,
            RichTextTags.Table,
            RichTextTags.TableRow,
            RichTextTags.TableHeaderCell,
            RichTextTags.TableCell,
            RichTextTags.Hr,
            RichTextTags.Heading2,
            RichTextTags.Heading3,
            RichTextTags.Heading4,
            RichTextTags.Heading5,
            RichTextTags.Heading6
        };

        foreach (var componentType in basicComponentTypes)
        {
            var testValue = new ContentItem
            {
                NodeType = componentType
            };

            var sut = GetService();
            var result = sut.MapContent(testValue);

            result.Should().NotBeNull();
            result!.NodeType.Should().Be(sut.ConvertToRichTextNodeType(componentType));
        }
    }

    [Fact]
    public void Unknown_Returns_Unknown()
    {
        var testValue = It.IsAny<string>();

        var sut = GetService();
        var result = sut.ConvertToRichTextNodeType(testValue);

        result.Should().Be(RichTextNodeType.Unknown);
    }

    [Fact]
    public void UnknownCustom_Returns_Null()
    {
        var testValue = new Target
        {
            SystemProperties = new SystemProperties
            {
                ContentType = new Contentful.Core.Models.ContentType
                {

                    SystemProperties = new SystemProperties
                    {
                        Id = "DUMMY"
                    }
                }
            }
        };

        var sut = GetService();
        var result = sut.GenerateCustomComponent(testValue);

        result.Should().BeNull();
    }

    [Fact]
    public void MapRichTextNodes_NullNode_Returns_UnknownItem()
    {
        const string internalName = "Internal Name";
        var nodes = new List<ContentItem>
        {
            new()
            {
                InternalName = internalName,
                NodeType = "DUMMY"
            }
        };

        var sut = GetService();

        var result = sut.MapRichTextNodes(nodes);
        result.Count.Should().Be(1);
        var richText = result[0];

        richText.NodeType.Should().Be(RichTextNodeType.Unknown);
        richText.InternalName.Should().Be(internalName);
    }
}
