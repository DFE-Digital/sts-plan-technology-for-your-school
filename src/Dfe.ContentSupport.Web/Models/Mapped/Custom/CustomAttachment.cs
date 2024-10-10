using System.Diagnostics.CodeAnalysis;
using Dfe.ContentSupport.Web.Models.Mapped.Types;

namespace Dfe.ContentSupport.Web.Models.Mapped.Custom;

[ExcludeFromCodeCoverage]
public class CustomAttachment : CustomComponent
{
    public CustomAttachment()
    {
        Type = CustomComponentType.Attachment;
    }

    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
    public string Uri { get; set; } = null!;
    public DateTime? UpdatedAt { get; set; }
}
