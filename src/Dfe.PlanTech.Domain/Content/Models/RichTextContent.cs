using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using System.Text;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Content for a 'RichText' field in Contentful
/// </summary>
/// <inheritdoc/>
public class RichTextContent : ContentComponent, IRichTextContent<RichTextMark, RichTextContent, RichTextData>
{
    public string Value { get; init; } = "";

    public string NodeType { get; init; } = "";

    public List<RichTextMark> Marks { get; init; } = new();

    public RichTextNodeType MappedNodeType
     => Enum.GetValues<RichTextNodeType>().FirstOrDefault(value => value.ToString().ToLower() == NodeType.Replace("-", ""));

    public List<RichTextContent> Content { get; init; } = new();

    public RichTextData? Data { get; init; }
}