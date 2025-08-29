using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class CAndSLinkEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string LinkText { get; set; } = null!;
}
