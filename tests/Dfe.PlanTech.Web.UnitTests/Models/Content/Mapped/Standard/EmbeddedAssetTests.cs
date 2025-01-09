using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Standard;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Types;
using Dfe.PlanTech.Web.Configuration;
using FluentAssertions;
using Xunit;
using FileDetails = Dfe.PlanTech.Domain.Content.Models.ContentSupport.FileDetails;

namespace Dfe.PlanTech.Web.UnitTests.Models.Content.Mapped.Standard;

public class EmbeddedAssetTests
{
    private static ModelMapper GetService(SupportedAssetTypes types) => new(types);

    private const string InternalName = "Internal Name";
    private const string ContentType = "Content Type";
    private const string Description = "Description";
    private const string Title = "Title";
    private const string Uri = "Uri";

    private static ContentItem DummyContentItem() => new()
    {
        NodeType = RichTextTags.EmbeddedAsset,
        InternalName = InternalName,
        Data = new Data
        {
            Target = new Target
            {
                Fields = new Fields
                {
                    File = new FileDetails
                    {
                        ContentType = ContentType,
                        Url = Uri
                    },
                    Description = Description,
                    Title = Title
                }
            }
        }
    };


    [Fact]
    public void ImageMapsCorrectly()
    {
        var testValue = DummyContentItem();
        var supportedTypes = new SupportedAssetTypes
        {
            ImageTypes = [ContentType],
            VideoTypes = []
        };

        var sut = GetService(supportedTypes);

        var result = sut.MapContent(testValue)!;
        result.Should().BeAssignableTo<EmbeddedAsset>();
        var asset = (result as EmbeddedAsset)!;

        asset.NodeType.Should().Be(RichTextNodeType.EmbeddedAsset);
        asset.AssetContentType.Should().Be(AssetContentType.Image);
        asset.InternalName.Should().Be(InternalName);
        asset.Description.Should().Be(Description);
        asset.Title.Should().Be(Title);
        asset.Uri.Should().Be(Uri);
    }

    [Fact]
    public void VideoMapsCorrectly()
    {
        var testValue = DummyContentItem();
        var supportedTypes = new SupportedAssetTypes
        {
            ImageTypes = [],
            VideoTypes = [ContentType]
        };

        var sut = GetService(supportedTypes);

        var result = sut.MapContent(testValue)!;
        result.Should().BeAssignableTo<EmbeddedAsset>();
        var asset = (result as EmbeddedAsset)!;

        asset.NodeType.Should().Be(RichTextNodeType.EmbeddedAsset);
        asset.AssetContentType.Should().Be(AssetContentType.Video);
        asset.InternalName.Should().Be(InternalName);
        asset.Description.Should().Be(Description);
        asset.Title.Should().Be(Title);
        asset.Uri.Should().Be(Uri);
    }

    [Fact]
    public void UnknownMapsCorrectly()
    {
        var testValue = DummyContentItem();
        var supportedTypes = new SupportedAssetTypes
        {
            ImageTypes = [],
            VideoTypes = []
        };

        var sut = GetService(supportedTypes);

        var result = sut.MapContent(testValue)!;
        result.Should().BeAssignableTo<EmbeddedAsset>();
        var asset = (result as EmbeddedAsset)!;

        asset.NodeType.Should().Be(RichTextNodeType.EmbeddedAsset);
        asset.AssetContentType.Should().Be(AssetContentType.Unknown);
        asset.InternalName.Should().Be(InternalName);
        asset.Description.Should().Be(Description);
        asset.Title.Should().Be(Title);
        asset.Uri.Should().Be(Uri);
    }
}
