namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Mark type for the rich text (e.g. bold, underline)
/// </summary>
public class RichTextMark
{
    public string Type { get; init; } = "";

    public MarkType MarkType => Enum.TryParse(Type, true, out MarkType markType) ? markType : MarkType.Unknown;
}