using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Domain.DataTransferObjects;

namespace Dfe.PlanTech.Infrastructure.Data.Entities;

public class QuestionEntity
{
    [Required]
    public int Id { get; init; }

    [Required]
    public string? QuestionText { get; init; } = null!;

    [Required]
    public string ContentfulRef { get; init; } = null!;

    [Required]
    public DateTime DateCreated { get; private set; } = DateTime.UtcNow;

    public QuestionDto ToDto()
    {
        return new QuestionDto
        {
            Id = Id,
            QuestionText = QuestionText,
            ContentfulRef = ContentfulRef,
            DateCreated = DateCreated
        };
    }
}
