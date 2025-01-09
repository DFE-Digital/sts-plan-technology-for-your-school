using Contentful.Core.Models;
using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Custom;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Standard;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Types;
using Dfe.PlanTech.Web.Configuration;
using FluentAssertions;
using Xunit;
using File = Contentful.Core.Models.File;
using FileDetails = Contentful.Core.Models.FileDetails;

namespace Dfe.PlanTech.Web.UnitTests.Models.Content.Mapped.Custom;

public class CustomAttachmentTests
{
    private static ModelMapper GetService() => new(new SupportedAssetTypes());

    private const string ContentId = "Attachment";
    private const string InternalName = "Internal Name";
    private const string ContentType = "Content Type";
    private const string Title = "Title";
    private const string Uri = "Uri";
    private const long Size = 123456789;

    private static ContentItem DummyContentItem() => new()
    {
        NodeType = RichTextTags.EmbeddedEntry,
        Data = new Data
        {
            Target = new Target
            {
                InternalName = InternalName,
                Title = Title,
                SystemProperties = new SystemProperties
                {
                    ContentType = new Contentful.Core.Models.ContentType
                    {

                        SystemProperties = new SystemProperties
                        {
                            Id = ContentId
                        }
                    }
                },
                Asset = new Asset
                {
                    File = new File
                    {
                        ContentType = ContentType,
                        Url = Uri,
                        Details = new FileDetails
                        {
                            Size = Size
                        },
                    },
                    SystemProperties = new SystemProperties
                    {
                        UpdatedAt = DateTime.Now
                    }
                }
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
        entry.InternalName.Should().Be(InternalName);
        entry.RichText.Should().BeNull();
        entry.CustomComponent.Should().NotBeNull();

        var customComponent = entry.CustomComponent;
        customComponent.Should().BeAssignableTo<CustomAttachment>();
        var attachment = (customComponent as CustomAttachment)!;

        attachment.Type.Should().Be(CustomComponentType.Attachment);
        attachment.InternalName.Should().Be(InternalName);
        attachment.ContentType.Should().Be(ContentType);
        attachment.Title.Should().Be(Title);
        attachment.Uri.Should().Be(Uri);
        attachment.Size.Should().Be(Size);
    }
}
