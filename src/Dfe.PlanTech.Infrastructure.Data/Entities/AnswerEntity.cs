using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Domain.DataTransferObjects;

namespace Dfe.PlanTech.Infrastructure.Data.Entities;

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

    public AnswerDto ToDto()
    {
        return new AnswerDto
        {
            Id = Id,
            AnswerText = AnswerText,
            ContentfulRef = ContentfulRef,
            DateCreated = DateCreated
        };
    }
}
