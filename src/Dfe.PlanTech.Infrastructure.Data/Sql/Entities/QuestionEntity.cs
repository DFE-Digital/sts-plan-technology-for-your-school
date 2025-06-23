using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Core.DataTransferObjects;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Entities;

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

    public SqlQuestionDto ToDto()
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
