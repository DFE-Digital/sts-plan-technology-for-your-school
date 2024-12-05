using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Web.Configuration;
using Dfe.PlanTech.Web.Content;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Types;
using FluentAssertions;
using Xunit;
using Hyperlink = Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Standard.Hyperlink;

namespace Dfe.PlanTech.Web.UnitTests.Models.Content.Mapped.Standard;

public class HyperlinkTests
{
    private static ModelMapper GetService() => new(new SupportedAssetTypes());

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
