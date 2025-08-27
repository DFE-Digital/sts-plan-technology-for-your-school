using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Interfaces;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class RichTextContentField : ContentfulField, IRichTextContent
{
    public string Value { get; set; } = "";
    public string NodeType { get; set; } = "";
    public List<RichTextMarkField> Marks { get; set; } = [];
    public List<RichTextContentField> Content { get; set; } = [];
    public RichTextContentSupportDataField? Data { get; set; }
}
