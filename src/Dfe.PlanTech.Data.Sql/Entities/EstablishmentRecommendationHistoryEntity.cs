using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Data.Sql.Entities;

[Table("question")]
public class EstablishmentRecommendationHistoryEntity
{
    public int EstablishmentId { get; init; }

    public int RecommendationId { get; init; }

    public int UserId { get; init; }

    public int? MatEstablishmentId { get; init; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public string? PreviousStatus { get; init; }

    public string NewStatus { get; init; } = null!;

    public string? NoteText { get; init; }

    public SqlEstablishmentRecommendationHistoryDto AsDto()
    {
        return new SqlEstablishmentRecommendationHistoryDto
        {
            EstablishmentId = EstablishmentId,
            RecommendationId = RecommendationId,
            UserId = UserId,
            MatEstablishmentId = MatEstablishmentId,
            DateCreated = DateCreated,
            PreviousStatus = PreviousStatus,
            NewStatus = NewStatus,
            NoteText = NoteText,
        };
    }
}
