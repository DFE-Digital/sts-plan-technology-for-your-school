using Dfe.PlanTech.Domain.Content.Enums;

namespace Dfe.PlanTech.Domain.Content.Models;

public class Header : ContentComponent
{
    public string Text { get; init; } = null!;

    public HeaderTag Tag { get; init; }

    public HeaderSize Size { get; init; }
}
