using Dfe.ContentSupport.Web.Common;
using Dfe.ContentSupport.Web.Configuration;
using Dfe.ContentSupport.Web.Models;
using Dfe.ContentSupport.Web.Models.Mapped.Types;
using Hyperlink = Dfe.ContentSupport.Web.Models.Mapped.Standard.Hyperlink;

namespace Dfe.ContentSupport.Web.Tests.Models.Mapped.Standard;

public class HyperlinkTests
{
    private static ModelMapper GetService() => new ModelMapper(new SupportedAssetTypes());

    private static ContentItem DummyContentItem(string uri) => new()
    {
        NodeType = RichTextTags.Hyperlink,
        Data = new Data
        {
            Uri = new Uri(uri)
        }
    };

    [Fact]
    public void IsVimeoMapsCorrectly()
    {
        const string uri = "https://www.vimeo.com/dummy";
        var testValue = DummyContentItem(uri);

        var sut = GetService();
        var result = sut.MapContent(testValue);
        result.Should().BeAssignableTo<Hyperlink>();
        var hyperlink = (result as Hyperlink)!;

        hyperlink.NodeType.Should().Be(RichTextNodeType.Hyperlink);
        hyperlink.InternalName.Should().BeNull();
        hyperlink.Uri.Should().Be(uri);
        hyperlink.IsVimeo.Should().BeTrue();
    }

    [Fact]
    public void NotVimeoMapsCorrectly()
    {
        const string uri = "https://www.youtube.com/dummy";
        var testValue = DummyContentItem(uri);

        var sut = GetService();
        var result = sut.MapContent(testValue);
        result.Should().BeAssignableTo<Hyperlink>();
        var hyperlink = (result as Hyperlink)!;

        hyperlink.NodeType.Should().Be(RichTextNodeType.Hyperlink);
        hyperlink.InternalName.Should().BeNull();
        hyperlink.Uri.Should().Be(uri);
        hyperlink.IsVimeo.Should().BeFalse();
    }
}
