using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Content.Models.ContentSupport;

[ExcludeFromCodeCoverage]
public class Sys
{
    public ContentType? ContentType { get; set; } = null!;
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
