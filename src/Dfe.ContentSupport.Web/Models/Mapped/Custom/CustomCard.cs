using System.Diagnostics.CodeAnalysis;
using Dfe.ContentSupport.Web.Models.Mapped.Types;

namespace Dfe.ContentSupport.Web.Models.Mapped.Custom;

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
