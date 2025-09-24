using System.Text.RegularExpressions;
using Dfe.PlanTech.Core.Contentful.Enums;

namespace Dfe.PlanTech.Core.Contentful.Interfaces;

/// <summary>
/// Content for a 'RichText' field in Contentful
/// </summary>
/// <inheritdoc/>
public partial interface IRichTextContent
{
    /// <summary>
    /// Actual value of this node.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// NodeType for This node; e.g. paragraph, underordered-list, etc.
    /// </summary>
    public string NodeType { get; set; }

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


public interface IRichTextContent<TMark, TContentType, TData> : IRichTextContent
    where TMark : IRichTextMark, new()
    where TContentType : IRichTextContent<TMark, TContentType, TData>, new()
    where TData : IHasUri, new()
{
    /// <summary>
    /// Collection of marks (e.g. underline, bold, etc.)
    /// </summary>
    public List<TMark> Marks { get; set; }

    /// <summary>
    /// Child content
    /// </summary>
    public List<TContentType> Content { get; set; }

    /// <summary>
    /// Additional information for the text (e.g. HTML URL for a link)
    /// </summary>
    public TData? Data { get; set; }
}
