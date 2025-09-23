using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

public class SqlRecommendationDto : ISqlDto
{
    public int Id { get; init; }
    public string? RecommendationText { get; init; } = null!;
    public string ContentfulSysId { get; init; } = null!;
    public DateTime DateCreated { get; init; } = DateTime.UtcNow;
    public int QuestionId { get; init; }
    public SqlQuestionDto Question { get; init; } = null!;
    public bool Archived { get; init; } = false;
}
