using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Entities;

public class AnswerEntity : SqlEntity<SqlAnswerDto>
{
    [Required]
    public int Id { get; init; }

    [Required]
    public string? AnswerText { get; init; } = null!;

    [Required]
    public string ContentfulSysId { get; init; } = null!;

    [Required]
    public DateTime DateCreated { get; private set; } = DateTime.UtcNow;

    protected override SqlAnswerDto CreateDto()
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
