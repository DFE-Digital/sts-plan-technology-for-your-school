using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class CsBodyTextEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public RichTextContentField RichText { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Subtitle { get; set; } = null!;
}
