using Dfe.ContentSupport.Web.Models.Mapped.Types;
using System.Diagnostics.CodeAnalysis;

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