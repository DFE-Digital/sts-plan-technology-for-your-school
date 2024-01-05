
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

public class WarningComponentDbEntity : ContentComponentDbEntity, IWarningComponent<TextBodyDbEntity>, IHasRichText
{
    public string TextId { get; set; } = null!;
    public TextBodyDbEntity Text { get; set; } = null!;

    public RichTextContentDbEntity RichText => Text.RichText;
    public long RichTextId => Text.RichTextId;
}