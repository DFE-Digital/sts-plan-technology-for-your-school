using System.Diagnostics.CodeAnalysis;

namespace Dfe.ContentSupport.Web.Configuration;

[ExcludeFromCodeCoverage]
public class TrackingOptions
{
    public string Gtm { get; set; } = null!;
    public string Clarity { get; set; } = null!;
}
