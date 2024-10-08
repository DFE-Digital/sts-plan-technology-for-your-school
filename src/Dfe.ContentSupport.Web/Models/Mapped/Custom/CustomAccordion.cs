using Dfe.ContentSupport.Web.Models.Mapped.Types;
using System.Diagnostics.CodeAnalysis;

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