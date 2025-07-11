using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Web.ViewModels;

namespace Dfe.PlanTech.Web.Models;

public class ChangeAnswersViewModel
{
    [Required]
    public string Title { get; init; } = null!;

    [Required]
    public string SectionName { get; init; } = null!;

    [Required]
    public SubmissionResponsesViewModel? SubmissionResponses { get; init; } = null!;

    public int? SubmissionId { get; init; }

    public string? SectionSlug { get; init; } = null;

    public string? Slug { get; init; } = null;

    public string? ErrorMessage { get; set; } = null;
}
