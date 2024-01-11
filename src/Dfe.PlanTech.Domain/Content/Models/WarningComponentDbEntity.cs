
using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

public class WarningComponentDbEntity : ContentComponentDbEntity, IWarningComponent<TextBodyDbEntity>, IHasRichText
{
    public string TextId { get; set; } = null!;
    public TextBodyDbEntity Text { get; set; } = null!;

    [NotMapped]
    public RichTextContentDbEntity RichText
    {
        get => Text.RichText;
        set => Text.RichText = value;
    }

    [NotMapped]
    public long RichTextId => Text.RichTextId;
}