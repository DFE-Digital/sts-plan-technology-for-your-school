using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface IRichTextContent
{
    string Value { get; init; }
    string NodeType { get; init; }
    RichTextMark[] Marks { get; init; }
    RichTextNodeType MappedNodeType { get; }
    RichTextContent[] Content { get; init; }
    RichTextData? Data { get; init; }
    string AsciiValue { get; }
}
