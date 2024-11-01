using System.Diagnostics.CodeAnalysis;
using Dfe.ContentSupport.Web.Models.Mapped.Types;

namespace Dfe.ContentSupport.Web.Models.Mapped.Custom;

[ExcludeFromCodeCoverage]
public class CustomAccordion : CustomComponent
{
    public CustomAccordion()
    {
        Type = CustomComponentType.Accordion;
    }

    public List<CustomAccordion> Accordions { get; set; } = null!;
    public RichTextContentItem? Body { get; set; }
    public string SummaryLine { get; set; } = null!;
}
