using Contentful.Core.Models;
using Dfe.ContentSupport.Web.Common;
using Dfe.ContentSupport.Web.Configuration;
using Dfe.ContentSupport.Web.Models;
using Dfe.ContentSupport.Web.Models.Mapped.Standard;
using Dfe.ContentSupport.Web.Models.Mapped.Types;

namespace Dfe.ContentSupport.Web.Tests.Models.Mapped.Standard;

public class CsTextTests
{
    private static IModelMapper GetService() => new ModelMapper(new SupportedAssetTypes());

    private const string InternalName = "Internal Name";

    private static ContentItem DummyContentItem(string mark) => new()
    {
        NodeType = RichTextTags.Text,
        InternalName = InternalName,
        Marks =
        [
            new Mark { Type = mark }
        ]
    };

    [Fact]
    public void BoldMapsCorrectly()
    {
        var testValue = DummyContentItem("bold");

        var sut = GetService();
        var result = sut.MapContent(testValue);
        result.Should().BeAssignableTo<CsText>();
        var text = (result as CsText)!;

        text.NodeType.Should().Be(RichTextNodeType.Text);
        text.InternalName.Should().Be(InternalName);
        text.IsBold.Should().BeTrue();
    }

    [Fact]
    public void NotBoldMapsCorrectly()
    {
        var testValue = DummyContentItem("dummy");

        var sut = GetService();
        var result = sut.MapContent(testValue);
        result.Should().BeAssignableTo<CsText>();
        var text = (result as CsText)!;

        text.NodeType.Should().Be(RichTextNodeType.Text);
        text.InternalName.Should().Be(InternalName);
        text.IsBold.Should().BeFalse();
    }
}