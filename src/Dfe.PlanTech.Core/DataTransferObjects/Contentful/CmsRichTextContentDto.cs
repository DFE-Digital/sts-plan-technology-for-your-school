using System.Text.RegularExpressions;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public partial class CmsRichTextContentDto : CmsEntryDto, IRichTextContent
{
    public string Value { get; set; } = "";
    public string NodeType { get; set; } = "";
    public List<CmsRichTextMarkDto> Marks { get; set; } = [];
    public List<CmsRichTextContentDto> Content { get; set; } = [];
    public CmsRichTextContentSupportDataDto? Data { get; set; }

    public CmsRichTextContentDto(RichTextContentEntry richTextContent)
    {
        Value = richTextContent.Value;
        NodeType = richTextContent.NodeType;
        Marks = richTextContent.Marks.Select(m => m.AsDto()).ToList();
        Content = richTextContent.Content.Select(c => c.AsDto()).ToList();
    }

    public RichTextNodeType MappedNodeType =>
        Array.Find(
            Enum.GetValues<RichTextNodeType>(),
            value =>
            {
                string? enumName = GetNameForNodeType(value);
                return MatchesNodeType(enumName);
            }
        );

    public bool MatchesNodeType(string? enumName)
    => string.Equals(enumName, RemoveHyphensAndNumbersRegEx().Replace(NodeType, ""), StringComparison.OrdinalIgnoreCase);

    public string? GetNameForNodeType(RichTextNodeType value) => Enum.GetName(value);

    [GeneratedRegex("-|\\d")]
    private static partial Regex RemoveHyphensAndNumbersRegEx();
}
