using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Data.Sql.Entities;

public class RecommendationEntity : IUserActionEntity
{
    public int Id { get; init; }

    public string ContentfulRef { get; init; } = null!;

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public string? RecommendationText { get; set; } = null!;

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
            UserActionId = UserActionId,
        };
    }

    public static RecommendationEntity BuildEntity(SqlRecommendationDto recommendationDto)
    {
        return new RecommendationEntity
        {
            ContentfulRef = recommendationDto.ContentfulSysId,
            RecommendationText = recommendationDto.RecommendationText,
            QuestionId = recommendationDto.QuestionId,
            QuestionContentfulRef = recommendationDto.QuestionContentfulRef,
            Archived = recommendationDto.Archived,
        };
    }
}
