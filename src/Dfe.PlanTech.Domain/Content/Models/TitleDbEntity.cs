using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Model for Title content type stored in DB
/// </summary>
public class TitleDbEntity : ContentComponentDbEntity, ITitle
{
    public string? Text { get; set; }

    public List<PageDbEntity> Pages { get; set; } = new();
}
