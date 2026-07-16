using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Entities;

[ExcludeFromCodeCoverage]
[Keyless]
public class FirstActivityForEstablishmentRecommendationEntity
{
    public DateTime StatusChangeDate { get; init; }

    public RecommendationStatus? Status { get; init; } = null!;

    public string SchoolName { get; init; } = null!;

    public string? GroupName { get; init; }

    public int UserId { get; init; }

    public string QuestionText { get; init; } = null!;

    public string AnswerText { get; init; } = null!;

    public SqlFirstActivityForEstablishmentRecommendationDto AsDto()
    {
        return new SqlFirstActivityForEstablishmentRecommendationDto
        {
            StatusChangeDate = StatusChangeDate,
            Status = Status ?? RecommendationStatus.NotStarted,
            SchoolName = SchoolName,
            GroupName = GroupName,
            UserId = UserId,
            QuestionText = QuestionText,
            AnswerText = AnswerText,
        };
    }
}
