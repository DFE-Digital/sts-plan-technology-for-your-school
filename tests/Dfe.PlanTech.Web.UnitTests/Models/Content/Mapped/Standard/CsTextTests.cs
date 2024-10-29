using Contentful.Core.Models;
using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Web.Configuration;
using Dfe.PlanTech.Web.Content;
using Dfe.PlanTech.Web.Models.Content;
using Dfe.PlanTech.Web.Models.Content.Mapped.Standard;
using Dfe.PlanTech.Web.Models.Content.Mapped.Types;
using FluentAssertions;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Models.Content.Mapped.Standard;

public class CsTextTests
{
    private static ModelMapper GetService() => new(new SupportedAssetTypes());

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
