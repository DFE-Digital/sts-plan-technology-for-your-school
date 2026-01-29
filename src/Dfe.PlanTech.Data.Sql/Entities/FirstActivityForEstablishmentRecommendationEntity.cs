using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Entities;

[ExcludeFromCodeCoverage]
[Keyless]
public class FirstActivityForEstablishmentRecommendationEntity
{
    public DateTime StatusChangeDate { get; init; }

    public string StatusText { get; init; } = null!;

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
            StatusText = StatusText,
            SchoolName = SchoolName,
            GroupName = GroupName,
            UserId = UserId,
            QuestionText = QuestionText,
            AnswerText = AnswerText,
        };
    }
}
