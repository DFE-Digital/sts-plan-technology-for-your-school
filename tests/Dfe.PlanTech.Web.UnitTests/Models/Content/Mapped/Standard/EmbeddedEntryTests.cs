using Contentful.Core.Models;
using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;

namespace Dfe.PlanTech.Web.UnitTests.Models.Content.Mapped.Standard;

public class EmbeddedEntryTests
{

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

}
