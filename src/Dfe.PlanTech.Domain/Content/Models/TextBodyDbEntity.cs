
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Model for TextBody content type table
/// </summary>
/// <inheritdoc/>
public class TextBodyDbEntity : ContentComponentDbEntity, ITextBody<RichTextContentDbEntity>
{
  public RichTextContentDbEntity RichText { get; set; } = null!;

  public string RichTextId { get; set; } = null!;
}