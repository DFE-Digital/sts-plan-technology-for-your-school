using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Domain.Content.Models.ContentSupport;

[ExcludeFromCodeCoverage]
public class Fields
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public FileDetails File { get; set; } = null!;
}
