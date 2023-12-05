using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

/// <summary>
/// Content for a 'RichText' field in Contentful
/// </summary>
/// <inheritdoc/>
public interface IRichTextContent
{
    /// <summary>
    /// Actual value of this node.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// NodeType for This node; e.g. paragraph, underordered-list, etc.
    /// </summary>
    public string NodeType { get; }
}

public interface IRichTextContent<TMark, TContentType, TData> : IRichTextContent
where TMark : IRichTextMark, new()
where TContentType : IRichTextContent<TMark, TContentType, TData>, new()
where TData : IRichTextData, new()
{
    /// <summary>
    /// Collection of marks (e.g. underline, bold, etc.)
    /// </summary>
    public List<TMark> Marks { get; }

    /// <summary>
    /// Maps NodeType field to Enum, for easier parsing in views
    /// </summary>
    /// <typeparam name="NodeTypes"></typeparam>
    public RichTextNodeType MappedNodeType { get; }

    /// <summary>
    /// Child content
    /// </summary>
    public List<TContentType> Content { get; }

    /// <summary>
    /// Additional information for the text (e.g. HTML URL for a link)
    /// </summary>
    public TData? Data { get; }
}