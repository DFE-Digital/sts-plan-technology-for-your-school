using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web.Models.Content;

[ExcludeFromCodeCoverage]
public class Sys
{
    public string Id { get; set; } = null!;
    public ContentType? ContentType { get; set; } = null!;
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
