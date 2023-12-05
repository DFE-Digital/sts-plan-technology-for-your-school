using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Model for the database table for the Header content type from Contentful
/// </summary>
/// <inheritdoc/>
public class HeaderDbEntity : ContentComponentDbEntity, IHeader
{
  public string Text { get; init; } = null!;

  public HeaderTag Tag { get; init; }

  public HeaderSize Size { get; init; }
}