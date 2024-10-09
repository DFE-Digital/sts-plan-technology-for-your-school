using Contentful.Core.Models;
using Dfe.ContentSupport.Web.Common;
using Dfe.ContentSupport.Web.Configuration;
using Dfe.ContentSupport.Web.Models;
using Dfe.ContentSupport.Web.Models.Mapped.Custom;
using Dfe.ContentSupport.Web.Models.Mapped.Standard;
using Dfe.ContentSupport.Web.Models.Mapped.Types;

namespace Dfe.ContentSupport.Web.Tests.Models.Mapped.Custom;

public class CustomGridContainerTests
{
    private static IModelMapper GetService() => new ModelMapper(new SupportedAssetTypes());

    private const string ContainerContentId = "GridContainer";
    private const string CardContentId = "csCard";
    private const string ContainerInternalName = "Container Internal Name";
    private const string CardInternalName = "Card Internal Name";
    private const string CardTitle = "Title";
    private const string CardDescription = "Description";
    private const string CardImageAlt = "Image Alt";
    private const string CardImageUri = "Image Uri";
    private const string CardMeta = "Meta";
    private const string CardUri = "Uri";

    private static readonly Target CardTarget = new()
    {
        InternalName = CardInternalName,
        Title = CardTitle,
        Uri = CardUri,
        Meta = CardMeta,
        ImageAlt = CardImageAlt,
        Description = CardDescription,
        SystemProperties = new SystemProperties
        {
            ContentType = new Contentful.Core.Models.ContentType
            {

                SystemProperties = new SystemProperties
                {
                    Id = CardContentId
                }
            }
        },
        Image = new Image
        {
            Fields = new Fields
            {
                File = new Web.Models.FileDetails
                {
                    Url = CardImageUri
                }
            }
        }
    };

    private static ContentItem DummyContentItem() => new()
    {
        NodeType = RichTextTags.EmbeddedEntry,
        Data = new Web.Models.Data
        {
            Target = new Target
            {
                InternalName = ContainerInternalName,
                SystemProperties = new Contentful.Core.Models.SystemProperties
                {
                    ContentType = new Contentful.Core.Models.ContentType
                    {
                        SystemProperties = new Contentful.Core.Models.SystemProperties
                        {
                            Id = ContainerContentId
                        }
                    }
                },
                Content =
                [
                    CardTarget
                ]
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
        entry.InternalName.Should().Be(ContainerInternalName);
        entry.RichText.Should().BeNull();
        entry.CustomComponent.Should().NotBeNull();

        var customComponent = entry.CustomComponent;
        customComponent.Should().BeAssignableTo<CustomGridContainer>();
        var gridContainer = (customComponent as CustomGridContainer)!;

        gridContainer.Type.Should().Be(CustomComponentType.GridContainer);
        gridContainer.InternalName.Should().Be(ContainerInternalName);
        gridContainer.Cards.Count.Should().Be(1);

        var card = gridContainer.Cards[0];

        card.Type.Should().Be(CustomComponentType.Card);
        card.InternalName.Should().Be(CardInternalName);
        card.Title.Should().Be(CardTitle);
        card.Description.Should().Be(CardDescription);
        card.ImageAlt.Should().Be(CardImageAlt);
        card.Meta.Should().Be(CardMeta);
        card.ImageUri.Should().Be(CardImageUri);
        card.Uri.Should().Be(CardUri);
    }
}
