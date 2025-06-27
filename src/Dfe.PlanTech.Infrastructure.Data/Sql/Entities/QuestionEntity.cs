using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Entities;

public class QuestionEntity : SqlEntity<SqlQuestionDto>
{
    public int Id { get; init; }

    public string? QuestionText { get; init; } = null!;

    public string ContentfulSysId { get; init; } = null!;

    public DateTime DateCreated { get; private set; } = DateTime.UtcNow;

    protected override SqlQuestionDto CreateDto()
    {
        return new SqlQuestionDto
        {
            Id = Id,
            QuestionText = QuestionText,
            ContentfulSysId = ContentfulSysId,
            DateCreated = DateCreated
        };
    }
}
