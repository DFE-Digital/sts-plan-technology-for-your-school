using System.Text;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Content for a 'RichText' field in Contentful
/// </summary>
public class RichTextContent : ContentComponent
{
    /// <summary>
    /// Actual value of this node.
    /// </summary>
    public string Value { get; init; } = "";

    /// <summary>
    /// NodeType for This node; e.g. paragraph, underordered-list, etc.
    /// </summary>
    public string NodeType { get; init; } = "";

    /// <summary>
    /// Collection of marks (e.g. underline, bold, etc.)
    /// </summary>
    public RichTextMark[] Marks { get; init; } = Array.Empty<RichTextMark>();

    /// <summary>
    /// Maps NodeType field to Enum, for easier parsing in views
    /// </summary>
    /// <typeparam name="NodeTypes"></typeparam>
    public RichTextNodeType MappedNodeType
     => Enum.GetValues<RichTextNodeType>().FirstOrDefault(value => nameof(value).ToLower() == NodeType.Replace("-", ""));

    public RichTextContent[] Content { get; init; } = Array.Empty<RichTextContent>();

    /// <summary>
    /// Strips non-ASCII (i.e. UTF) characters from Value field
    /// </summary>
    public string AsciiValue => Encoding.ASCII.GetString(
        Encoding.Convert(
            Encoding.UTF8,
            Encoding.GetEncoding(
                Encoding.ASCII.EncodingName,
                new EncoderReplacementFallback(string.Empty),
                new DecoderExceptionFallback()
                ),
            Encoding.UTF8.GetBytes(Value)
        ));
}