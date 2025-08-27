using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public abstract class ContentfulField
{
    public string Description { get; init; } = null!;
}
