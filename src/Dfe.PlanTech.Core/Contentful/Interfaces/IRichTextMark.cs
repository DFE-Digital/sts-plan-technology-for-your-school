using Dfe.PlanTech.Core.Contentful.Enums;

namespace Dfe.PlanTech.Core.Contentful.Interfaces;

/// <summary>
/// Mark type for the rich text (e.g. bold, underline)
/// </summary>
public interface IRichTextMark
{
    public string Type { get; set; }

    public MarkType MarkType { get; }
}
