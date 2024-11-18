using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Web.Models.Content.Mapped.Types;

namespace Dfe.PlanTech.Web.Models.Content.Mapped.Custom;

[ExcludeFromCodeCoverage]
public class CustomCard : CustomComponent
{
    public CustomCard()
    {
        Type = CustomComponentType.Card;
    }

    public string Description { get; set; } = null!;
    public string ImageAlt { get; set; } = null!;
    public string ImageUri { get; set; } = null!;
    public string Meta { get; set; } = null!;
    public string Uri { get; set; } = null!;
}
