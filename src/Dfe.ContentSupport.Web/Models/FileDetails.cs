using System.Diagnostics.CodeAnalysis;

namespace Dfe.ContentSupport.Web.Models;

[ExcludeFromCodeCoverage]
public class FileDetails
{
    public string Url { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}