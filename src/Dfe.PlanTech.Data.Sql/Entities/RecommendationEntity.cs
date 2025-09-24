using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Data.Sql.Entities;

[Table("recommendation")]
public class RecommendationEntity
{
    public int Id { get; init; }

    public string? RecommendationText { get; set; } = null!;

    public string ContentfulRef { get; init; } = null!;

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public string QuestionContentfulRef { get; init; } = null!;

    public bool Archived { get; set; } = false;

    public SqlRecommendationDto AsDto()
    {
        return new SqlRecommendationDto
        {
            Id = Id,
            RecommendationText = RecommendationText,
            ContentfulSysId = ContentfulRef,
            DateCreated = DateCreated,
            QuestionContentfulRef = QuestionContentfulRef,
            Archived = Archived
        };
    }
}
