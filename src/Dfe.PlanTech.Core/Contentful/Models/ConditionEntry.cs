using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class ConditionEntry : ContentfulEntry
{
    public string InternalName { get; init; } = null!;
    public ContentfulEntry Entry { get; init; } = null!;
    public bool? ShowIfStatusUnknown { get; init; } = null!;
    public bool? ShowIfNotStarted { get; init; } = null!;
    public bool? ShowIfInProgress { get; init; } = null!;
    public bool? ShowIfCompleted { get; init; } = null!;
}
