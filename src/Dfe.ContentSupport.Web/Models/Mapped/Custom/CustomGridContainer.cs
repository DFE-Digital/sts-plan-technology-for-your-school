using System.Diagnostics.CodeAnalysis;
using Dfe.ContentSupport.Web.Models.Mapped.Types;

namespace Dfe.ContentSupport.Web.Models.Mapped.Custom;

[ExcludeFromCodeCoverage]
public class CustomGridContainer : CustomComponent
{
    public CustomGridContainer()
    {
        Type = CustomComponentType.GridContainer;
    }

    public List<CustomCard> Cards { get; set; } = null!;
}
