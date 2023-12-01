using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Database.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Model for Title content type stored in DB
/// </summary>
public class TitleDbEntity : IDbEntity, ITitle
{
    public long Id { get; set; }

    public string ContentfulId { get; set; } = null!;

    public string Text { get; set; } = null!;
}