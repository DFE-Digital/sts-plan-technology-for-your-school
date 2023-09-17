using Dfe.PlanTech.Domain.Questionnaire.Models;
using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Web.Models;

public class QuestionViewModel
{
    [Required]
    public Question Question { get; init; } = null!;

    public string? AnswerRef { get; init; }

    public string? ErrorMessage { get; init; }

    public string SectionName { get; init; } = null!;

    public string SectionSlug { get; init; } = null!;

    public string SectionId { get; init; } = null!;
}
