using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Data.Sql.Entities;

[Table("question")]
public class QuestionEntity : IUserActionEntity
{
    public int Id { get; init; }

    public string? QuestionText { get; init; } = null!;

    public string ContentfulRef { get; init; } = null!;

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public int? Order { get; set; }

    public Guid? UserActionId { get; set; }

    public SqlQuestionDto AsDto()
    {
        return new SqlQuestionDto
        {
            Id = Id,
            QuestionText = QuestionText,
            ContentfulSysId = ContentfulRef,
            DateCreated = DateCreated,
            Order = Order,
        };
    }
}
