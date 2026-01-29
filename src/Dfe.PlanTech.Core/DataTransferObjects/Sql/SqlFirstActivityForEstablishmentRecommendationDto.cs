namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

public class SqlFirstActivityForEstablishmentRecommendationDto : ISqlDto
{
    public DateTime StatusChangeDate { get; init; }

    public string StatusText { get; init; } = null!;

    public string SchoolName { get; init; } = null!;

    public string? GroupName { get; init; }

    public int UserId { get; init; }

    public string QuestionText { get; init; } = null!;

    public string AnswerText { get; init; } = null!;
}
