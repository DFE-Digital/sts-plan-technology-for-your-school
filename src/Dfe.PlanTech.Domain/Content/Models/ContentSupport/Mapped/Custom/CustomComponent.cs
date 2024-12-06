using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Types;

namespace Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Custom;

[ExcludeFromCodeCoverage]
public class CustomComponent : CsContentItem
{
    public CustomComponentType Type { get; set; }
}
