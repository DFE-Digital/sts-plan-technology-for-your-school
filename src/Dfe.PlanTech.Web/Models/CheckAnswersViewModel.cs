using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Web.Models;

public class CheckAnswersViewModel
{
    [Required]
    public Title Title { get; init; } = null!;

    [Required]
    public string SectionName { get; init; } = null!;

    [Required]
    public ResponsesForSubmissionDto CheckAnswerDto { get; init; } = null!;

    [Required]
    public List<ContentComponent> Content { get; init; } = null!;

    public int? SubmissionId { get; init; }

    public string? SectionSlug { get; init; } = null;

    public string? Slug { get; init; } = null;

    public string? ErrorMessage { get; set; } = null;
}