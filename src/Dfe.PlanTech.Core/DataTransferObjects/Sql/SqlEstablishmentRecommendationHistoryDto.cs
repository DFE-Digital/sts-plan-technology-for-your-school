using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

public class SqlEstablishmentRecommendationHistoryDto : ISqlDto
{
    public int EstablishmentId { get; init; }
    public int RecommendationId { get; init; }
    public int UserId { get; init; }
    public int? MatEstablishmentId { get; init; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public RecommendationStatus? PreviousStatus { get; init; }
    public RecommendationStatus? NewStatus { get; init; }
    public string? NoteText { get; init; }
}
