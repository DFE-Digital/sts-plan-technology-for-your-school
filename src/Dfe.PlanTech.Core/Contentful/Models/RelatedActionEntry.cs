using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class RelatedActionEntry : ContentfulEntry
{
    public string? Title { get; set; }
    public string? Url { get; set; }
}
