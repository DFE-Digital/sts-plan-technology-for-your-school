using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Types;

namespace Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Custom;

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
    public string FileExtension { get; set; }
}
