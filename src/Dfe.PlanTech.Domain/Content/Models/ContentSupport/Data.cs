using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Domain.Content.Models.ContentSupport;

[ExcludeFromCodeCoverage]
public class Data
{
    public Target Target { get; set; } = null!;
    public Uri Uri { get; set; } = null!;
}
