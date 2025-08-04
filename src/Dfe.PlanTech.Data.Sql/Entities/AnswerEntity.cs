using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Data.Sql.Entities;

[Table("answer")]
public class AnswerEntity
{
    [Required]
    public int Id { get; init; }

    [Required]
    public string? AnswerText { get; init; } = null!;

    [Required]
    public string ContentfulSysId { get; init; } = null!;

    [Required]
    public DateTime DateCreated { get; private set; } = DateTime.UtcNow;

    public SqlAnswerDto AsDto()
    {
        return new SqlAnswerDto
        {
            Id = Id,
            AnswerText = AnswerText,
            ContentfulSysId = ContentfulSysId,
            DateCreated = DateCreated
        };
    }
}
