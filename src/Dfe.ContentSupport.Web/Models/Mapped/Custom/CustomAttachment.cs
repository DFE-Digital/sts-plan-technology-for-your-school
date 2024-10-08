using Dfe.ContentSupport.Web.Models.Mapped.Types;
using System.Diagnostics.CodeAnalysis;

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