using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Data.Sql.Entities;

public class FirstActivityForEstablishmentRecommendationEntity
{
    public DateTime StatusChangeDate { get; init; }

    public string StatusText { get; init; }

    public string SchoolName { get; init; } = null!;

    public string GroupName { get; init; } = null!;

    public int UserId { get; init; }

    public string QuestionText { get; init; } = null!;

    public string AnswerText { get; init; } = null!;

    public SqlFirstActivityForEstablishmentRecommendationDto AsDto()
    {
        return new SqlFirstActivityForEstablishmentRecommendationDto
        {
            StatusChangeDate = StatusChangeDate,
            StatusText = StatusText,
            SchoolName = SchoolName,
            GroupName = GroupName,
            UserId = UserId,
            QuestionText = QuestionText,
            AnswerText = AnswerText,
        };
    }
}
