using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Core.Content.Models;

/// <summary>
/// Model for Header content type from Contentful
/// </summary>
/// <inheritdoc/>
public class Header : ContentComponent, IHeader
{
    public string Text { get; init; } = null!;

    public HeaderTag Tag { get; init; }

    public HeaderSize Size { get; init; }
}
