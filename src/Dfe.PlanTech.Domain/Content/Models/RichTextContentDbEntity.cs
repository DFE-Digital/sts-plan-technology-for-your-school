using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Content for a 'RichText' field in Contentful
/// </summary>
/// <inheritdoc/>
public class RichTextContentDbEntity : ContentComponentDbEntity, IRichTextContent<RichTextMarkDbEntity, RichTextContentDbEntity, RichTextDataDbEntity>
{
    public string Value { get; set; } = "";

    public string NodeType { get; set; } = "";

    public List<RichTextMarkDbEntity> Marks { get; set; } = new();

    public RichTextNodeType MappedNodeType
     => Enum.GetValues<RichTextNodeType>().FirstOrDefault(value => value.ToString().ToLower() == NodeType.Replace("-", ""));

    public List<RichTextContentDbEntity> Content { get; set; } = new();

    public RichTextDataDbEntity? Data { get; set; }
}