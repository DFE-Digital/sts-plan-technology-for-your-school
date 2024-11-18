using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Web.Models.Content.Mapped.Types;

namespace Dfe.PlanTech.Web.Models.Content.Mapped.Custom;

[ExcludeFromCodeCoverage]
public class CustomGridContainer : CustomComponent
{
    public CustomGridContainer()
    {
        Type = CustomComponentType.GridContainer;
    }

    public List<CustomCard> Cards { get; set; } = null!;
}
