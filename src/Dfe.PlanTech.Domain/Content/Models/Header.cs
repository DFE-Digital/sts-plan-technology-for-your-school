using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Model for Header content type from Contentful
/// </summary>
/// <inheritdoc/>
public class Header : ContentComponent, IHeader
{
    public string Text { get; init; } = null!;

    public HeaderTag Tag { get; set; }
    public HeaderSize Size { get; set; }

    public void OverrideHeaderParams(HeaderTag tag, HeaderSize size, string text = null)
    {
        Tag = tag;
        Size = size;
    }
}
