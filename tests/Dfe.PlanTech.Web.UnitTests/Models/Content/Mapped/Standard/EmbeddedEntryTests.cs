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

public class EmbeddedEntryTests
{
    private static ModelMapper GetService() => new(new SupportedAssetTypes());

    private const string InternalName = "Internal Name";
    private const string JumpIdentifier = "JumpIdentifier";

    private static ContentItem DummyContentItem() => new()
    {
        NodeType = RichTextTags.EmbeddedEntry,
        Data = new Data
        {
            Target = new Target
            {
                InternalName = InternalName,
                JumpIdentifier = JumpIdentifier,
                SystemProperties = new SystemProperties()
            }
        }
    };

    [Fact]
    public void MapsCorrectly()
    {
        var testValue = DummyContentItem();

        var sut = GetService();
        var result = sut.MapContent(testValue);
        result.Should().BeAssignableTo<EmbeddedEntry>();
        var entry = (result as EmbeddedEntry)!;

        entry.NodeType.Should().Be(RichTextNodeType.EmbeddedEntry);
        entry.InternalName.Should().Be(InternalName);
        entry.JumpIdentifier.Should().Be(JumpIdentifier);
        entry.RichText.Should().BeNull();
        entry.CustomComponent.Should().BeNull();
    }
}
