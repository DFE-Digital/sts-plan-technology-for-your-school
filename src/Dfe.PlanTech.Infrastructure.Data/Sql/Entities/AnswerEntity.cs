using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Core.DataTransferObjects;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Entities;

public class AnswerEntity
{
    [Required]
    public int Id { get; init; }

    [Required]
    public string? AnswerText { get; init; } = null!;

    [Required]
    public string ContentfulRef { get; init; } = null!;

    [Required]
    public DateTime DateCreated { get; private set; } = DateTime.UtcNow;

    public SqlAnswerDto ToDto()
    {
        return new SqlAnswerDto
        {
            Id = Id,
            AnswerText = AnswerText,
            ContentfulRef = ContentfulRef,
            DateCreated = DateCreated
        };
    }
}
