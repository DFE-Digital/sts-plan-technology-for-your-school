using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;

namespace Dfe.PlanTech.Web.Models;

public class ChangeAnswersViewModel
{
    [Required]
    public Title Title { get; init; } = null!;

    [Required]
    public string SectionName { get; init; } = null!;

    [Required]
    public SubmissionResponsesDto SubmissionResponses { get; init; } = null!;

    public int? SubmissionId { get; init; }

    public string? SectionSlug { get; init; } = null;

    public string? Slug { get; init; } = null;

    public string? ErrorMessage { get; set; } = null;
}
