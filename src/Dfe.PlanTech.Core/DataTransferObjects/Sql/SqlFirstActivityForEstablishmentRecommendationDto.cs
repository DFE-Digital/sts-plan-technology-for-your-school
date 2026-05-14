using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

[ExcludeFromCodeCoverage]
public class SqlFirstActivityForEstablishmentRecommendationDto : ISqlDto
{
    public DateTime StatusChangeDate { get; init; }

    public RecommendationStatus Status { get; init; }

    public string SchoolName { get; init; } = null!;

    public string? GroupName { get; init; }

    public int UserId { get; init; }

    public string QuestionText { get; init; } = null!;

    public string AnswerText { get; init; } = null!;
}
