using Contentful.Core.Models;
using Dfe.PlanTech.Domain.Content.Enums;

namespace Dfe.PlanTech.Core.Content.Models;

/// <summary>
/// Model for Header content type from Contentful
/// </summary>
/// <inheritdoc/>
public class ComponentHeaderEntry : Entry<ContentComponent>
{
    public string Text { get; init; } = null!;
    public HeaderTag Tag { get; init; }
    public HeaderSize Size { get; init; }
}
