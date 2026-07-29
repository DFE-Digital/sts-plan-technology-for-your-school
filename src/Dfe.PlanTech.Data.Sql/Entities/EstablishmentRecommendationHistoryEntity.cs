using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Data.Sql.Entities;

public class EstablishmentRecommendationHistoryEntity : IUserActionEntity
{
    public int Id { get; init; } // New identity primary key

    public int EstablishmentId { get; init; }
    public EstablishmentEntity Establishment { get; set; } = null!;

    public int RecommendationId { get; init; }
    public RecommendationEntity Recommendation { get; set; } = null!;

    public int UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public int? MatEstablishmentId { get; init; }
    public EstablishmentEntity? MatEstablishment { get; set; }

    public ResponseEntity? Response { get; set; }
    public int? ResponseId { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public RecommendationStatus? PreviousStatus { get; set; }

    public RecommendationStatus? NewStatus { get; set; } = null!;

    public string? NoteText { get; set; }

    public Guid? UserActionId { get; set; }

    public SqlEstablishmentRecommendationHistoryDto AsDto()
    {
        return new SqlEstablishmentRecommendationHistoryDto
        {
            EstablishmentId = EstablishmentId,
            RecommendationId = RecommendationId,
            UserId = UserId,
            MatEstablishmentId = MatEstablishmentId,
            ResponseId = ResponseId,
            DateCreated = DateCreated,
            PreviousStatus = PreviousStatus,
            NewStatus = NewStatus,
            NoteText = NoteText,
            UserActionId = UserActionId,
        };
    }
}
