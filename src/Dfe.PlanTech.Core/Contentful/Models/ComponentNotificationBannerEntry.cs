using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class ComponentNotificationBannerEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public ComponentTextBodyEntry Text { get; init; } = null!;
}
