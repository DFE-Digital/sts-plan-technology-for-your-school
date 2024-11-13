using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web.Models.Content;

[ExcludeFromCodeCoverage]
public class FileDetails
{
    public string Url { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}
