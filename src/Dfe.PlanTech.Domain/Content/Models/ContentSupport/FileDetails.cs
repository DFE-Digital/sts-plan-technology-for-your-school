using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Domain.Content.Models.ContentSupport;

[ExcludeFromCodeCoverage]
public class FileDetails
{
    public string Url { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}
