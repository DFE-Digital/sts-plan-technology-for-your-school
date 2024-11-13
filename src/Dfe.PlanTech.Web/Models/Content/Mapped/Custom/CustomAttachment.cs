using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Web.Models.Content.Mapped.Types;

namespace Dfe.PlanTech.Web.Models.Content.Mapped.Custom;

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
