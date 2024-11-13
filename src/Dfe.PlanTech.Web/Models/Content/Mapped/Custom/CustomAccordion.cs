using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Web.Models.Content.Mapped.Types;

namespace Dfe.PlanTech.Web.Models.Content.Mapped.Custom;

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
