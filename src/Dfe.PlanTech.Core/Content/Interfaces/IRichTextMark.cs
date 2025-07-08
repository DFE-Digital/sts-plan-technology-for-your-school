using Dfe.PlanTech.Domain.Content.Enums;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

/// <summary>
/// Mark type for the rich text (e.g. bold, underline)
/// </summary>
public interface IRichTextMark
{
    public string Type { get; set; }

    public MarkType MarkType { get; }
}
