
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Model for TextBody content type table
/// </summary>
/// <inheritdoc/>
public class TextBodyDbEntity : ContentComponentDbEntity, ITextBody<RichTextContentDbEntity>, IHasRichText
{
    public RichTextContentDbEntity? RichText { get; set; }

    public long? RichTextId { get; set; }

    public List<WarningComponentDbEntity> Warnings { get; set; } = new();
}
