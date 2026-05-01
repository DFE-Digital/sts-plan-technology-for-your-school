using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Data.Sql.Entities;

[Table("recommendation")]
public class RecommendationEntity : IUserActionEntity
{
    public int Id { get; init; }

    public string? RecommendationText { get; set; } = null!;

    public string ContentfulRef { get; init; } = null!;

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public int QuestionId { get; init; }

    public QuestionEntity Question { get; init; } = null!;

    public bool Archived { get; set; } = false;

    public string? QuestionContentfulRef { get; init; }

    public Guid? UserActionId { get; set; }

    public SqlRecommendationDto AsDto()
    {
        return new SqlRecommendationDto
        {
            Id = Id,
            RecommendationText = RecommendationText,
            ContentfulSysId = ContentfulRef,
            DateCreated = DateCreated,
            QuestionId = QuestionId,
            Question = Question.AsDto(),
            Archived = Archived,
            QuestionContentfulRef = QuestionContentfulRef,
            UserActionId = UserActionId
        };
    }
}
