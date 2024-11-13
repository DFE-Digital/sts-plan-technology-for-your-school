using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Web.Models.Content.Mapped.Types;

namespace Dfe.PlanTech.Web.Models.Content.Mapped.Custom;

[ExcludeFromCodeCoverage]
public class CustomComponent : CsContentItem
{
    public CustomComponentType Type { get; set; }
}
