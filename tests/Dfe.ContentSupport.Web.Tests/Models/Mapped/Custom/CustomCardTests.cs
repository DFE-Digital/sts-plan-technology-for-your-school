using Dfe.ContentSupport.Web.Common;
using Dfe.ContentSupport.Web.Configuration;
using Dfe.ContentSupport.Web.Models;
using Dfe.ContentSupport.Web.Models.Mapped.Custom;
using Dfe.ContentSupport.Web.Models.Mapped.Standard;
using Dfe.ContentSupport.Web.Models.Mapped.Types;
using ContentType = Dfe.ContentSupport.Web.Models.ContentType;

namespace Dfe.ContentSupport.Web.Tests.Models.Mapped.Custom;

public class CustomCardTests
{
    private static IModelMapper GetService() => new ModelMapper(new SupportedAssetTypes());

    private const string ContentId = "csCard";
    private const string InternalName = "Internal Name";
    private const string Title = "Title";
    private const string Description = "Description";
    private const string ImageAlt = "Image Alt";
    private const string ImageUri = "Image Uri";
    private const string Meta = "Meta";
    private const string Uri = "Uri";

    private static ContentItem DummyContentItem() => new()
    {
        NodeType = RichTextTags.EmbeddedEntry,
        Data = new Data
        {
            Target = new Target
            {
                InternalName = InternalName,
                Title = Title,
                Uri = Uri,
                Meta = Meta,
                ImageAlt = ImageAlt,
                Description = Description,
                SystemProperties = new Contentful.Core.Models.SystemProperties
                {
                    ContentType = new Contentful.Core.Models.ContentType
                    {
                        SystemProperties = new Contentful.Core.Models.SystemProperties
                        {
                            Id = ContentId
                        }
                    }
                },
                Image = new Image
                {
                    Fields = new Fields
                    {
                        File = new FileDetails
                        {
                            Url = ImageUri
                        }
                    },
                }
            },
        },
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
        entry.RichText.Should().BeNull();
        entry.CustomComponent.Should().NotBeNull();

        var customComponent = entry.CustomComponent;
        customComponent.Should().BeAssignableTo<CustomCard>();
        var card = (customComponent as CustomCard)!;

        card.Type.Should().Be(CustomComponentType.Card);
        card.InternalName.Should().Be(InternalName);
        card.Title.Should().Be(Title);
        card.Description.Should().Be(Description);
        card.ImageAlt.Should().Be(ImageAlt);
        card.Meta.Should().Be(Meta);
        card.ImageUri.Should().Be(ImageUri);
        card.Uri.Should().Be(Uri);
    }
}