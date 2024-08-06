using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Mark type for the rich text (e.g. bold, underline)
/// </summary>
/// <inheritdoc/>
public class RichTextMarkDbEntity : IRichTextMark
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string Type { get; set; } = "";

    public MarkType MarkType => Enum.TryParse(Type, true, out MarkType markType) ? markType : MarkType.Unknown;
}
