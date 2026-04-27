using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Data.Sql.Entities;

[Table("answer")]
public class AnswerEntity : IUserActionEntity
{
    [Required]
    public int Id { get; init; }

    [Required]
    public string? AnswerText { get; init; } = null!;

    [Required]
    public string ContentfulRef { get; init; } = null!;

    [Required]
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public Guid? UserActionId { get; set; }

    public SqlAnswerDto AsDto()
    {
        return new SqlAnswerDto
        {
            Id = Id,
            AnswerText = AnswerText,
            ContentfulSysId = ContentfulRef,
            DateCreated = DateCreated,
            UserActionId = UserActionId
        };
    }
}
