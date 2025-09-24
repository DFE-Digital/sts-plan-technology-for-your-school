using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Data.Sql.Entities;

[Table("question")]
public class QuestionEntity
{
    public int Id { get; init; }

    public string? QuestionText { get; init; } = null!;

    public string ContentfulRef { get; init; } = null!;

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public SqlQuestionDto AsDto()
    {
        return new SqlQuestionDto
        {
            Id = Id,
            QuestionText = QuestionText,
            ContentfulSysId = ContentfulRef,
            DateCreated = DateCreated
        };
    }
}
