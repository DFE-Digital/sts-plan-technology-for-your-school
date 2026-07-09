using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class ComponentNotificationBannerEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public ComponentTextBodyEntry Text { get; init; } = null!;
    public DateTime? DisplayFrom { get; init; } = null!;
    public DateTime? DisplayTo { get; init; } = null!;
    public int? NumberOfTimesToShow { get; init; } = null!;
    public bool? ShowToSchoolUsers { get; init; } = null!;
    public bool? ShowToGroupUsers { get; init; } = null!;
    public IEnumerable<ConditionEntry> Conditions { get; init; } = null!;
}
