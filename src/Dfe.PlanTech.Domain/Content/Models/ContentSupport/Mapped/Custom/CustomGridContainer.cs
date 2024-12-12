using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Types;

namespace Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Custom;

[ExcludeFromCodeCoverage]
public class CustomGridContainer : CustomComponent
{
    public CustomGridContainer()
    {
        Type = CustomComponentType.GridContainer;
    }

    public List<CustomCard> Cards { get; set; } = null!;
}
