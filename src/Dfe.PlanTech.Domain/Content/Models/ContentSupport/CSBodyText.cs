using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Domain.Content.Models.ContentSupport;

[ExcludeFromCodeCoverage]
public class CSBodyText : Target
{
    public RichTextContent RichText { get; set; } = null!;
}
