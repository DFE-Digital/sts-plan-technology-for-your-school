using System.Diagnostics.CodeAnalysis;

namespace Dfe.ContentSupport.Web.Models;

[ExcludeFromCodeCoverage]
public class Data
{
    public Target Target { get; set; } = null!;
    public Uri Uri { get; set; } = null!;
}
