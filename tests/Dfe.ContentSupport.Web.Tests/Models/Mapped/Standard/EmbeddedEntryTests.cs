using Contentful.Core.Models;
using Dfe.ContentSupport.Web.Common;
using Dfe.ContentSupport.Web.Configuration;
using Dfe.ContentSupport.Web.Models;
using Dfe.ContentSupport.Web.Models.Mapped.Standard;
using Dfe.ContentSupport.Web.Models.Mapped.Types;

namespace Dfe.ContentSupport.Web.Tests.Models.Mapped.Standard;

public class EmbeddedEntryTests
{
    private static IModelMapper GetService() => new ModelMapper(new SupportedAssetTypes());

    private const string InternalName = "Internal Name";
    private const string JumpIdentifier = "JumpIdentifier";

    private static ContentItem DummyContentItem() => new()
    {
        NodeType = RichTextTags.EmbeddedEntry,
        Data = new Web.Models.Data
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
