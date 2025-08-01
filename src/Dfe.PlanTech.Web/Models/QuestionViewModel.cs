using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web.Models;

public class QuestionViewModel
{
    [Required]
    public Question Question { get; init; } = null!;

    public string? AnswerRef { get; init; }

    public IEnumerable<string>? ErrorMessages { get; set; }

    public string? SectionName { get; init; }

    public string? SectionSlug { get; init; }

    public string? SectionId { get; init; }

    public string? CategorySlug { get; init; }
}
