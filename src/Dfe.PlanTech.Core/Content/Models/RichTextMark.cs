using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Core.Content.Models;

/// <summary>
/// Mark type for the rich text (e.g. bold, underline)
/// </summary>
/// <inheritdoc/>
public class RichTextMark : IRichTextMark
{
    public string Type { get; set; } = "";

    public MarkType MarkType => Enum.TryParse(Type, true, out MarkType markType) ? markType : MarkType.Unknown;
}
