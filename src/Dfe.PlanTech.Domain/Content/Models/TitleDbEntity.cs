using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Model for Title content type stored in DB
/// </summary>
public class TitleDbEntity : ContentComponentDbEntity, ITitle
{
    public string InternalName { get; set; } = null!;

    public string Text { get; set; } = null!;

    public List<PageDbEntity> Pages { get; set; } = new();
}
