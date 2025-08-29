using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class RichTextContentSupportDataField : ContentfulField
{
    public string? Uri { get; init; }
    public RichTextContentDataEntry? Target { get; init; }
}
