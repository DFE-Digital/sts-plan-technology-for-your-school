using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Core.Helpers;

namespace Dfe.PlanTech.Data.Sql.Entities;

[Table("establishmentRecommendationHistory")]
public class EstablishmentRecommendationHistoryEntity
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

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public string? PreviousStatus { get; set; }

    public string NewStatus { get; set; } = null!;

    public string? NoteText { get; set; }

    public SqlEstablishmentRecommendationHistoryDto AsDto()
    {
        return new SqlEstablishmentRecommendationHistoryDto
        {
            EstablishmentId = EstablishmentId,
            RecommendationId = RecommendationId,
            UserId = UserId,
            MatEstablishmentId = MatEstablishmentId,
            DateCreated = DateCreated,
            PreviousStatus = PreviousStatus is null
                ? null
                : PreviousStatus.GetRecommendationStatusEnumValue()!.GetDisplayName(),
            NewStatus = NewStatus.GetRecommendationStatusEnumValue()!.GetDisplayName(),
            NoteText = NoteText,
        };
    }
}
