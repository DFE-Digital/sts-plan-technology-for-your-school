namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

public class SqlEstablishmentRecommendationHistoryDto : ISqlDto
{
    public int EstablishmentId { get; init; }
    public int RecommendationId { get; init; }
    public int UserId { get; init; }
    public int? MatEstablishmentId { get; init; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public string? PreviousStatus { get; init; }
    public string NewStatus { get; init; } = null!;
    public string? NoteText { get; init; }
}
