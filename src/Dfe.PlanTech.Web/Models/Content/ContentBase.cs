using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web.Models.Content;

[ExcludeFromCodeCoverage]
public class ContentBase : Contentful.Core.Models.Entry<ContentBase>
{
    public string InternalName { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
}
