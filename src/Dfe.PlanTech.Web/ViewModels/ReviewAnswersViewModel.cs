using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewModels;

public class ReviewAnswersViewModel
{
    [Required]
    public string SectionName { get; init; } = null!;

    [Required]
    public SubmissionResponsesViewModel? SubmissionResponses { get; init; } = null!;

    [Required]
    public ComponentTitleEntry Title { get; init; } = null!;

    public List<ContentfulEntry>? Content { get; init; }
    public string? CategorySlug { get; init; } = null;
    public string? ErrorMessage { get; set; } = null;
    public string? SectionSlug { get; init; } = null;
    public string? Slug { get; init; } = null;
    public int? SubmissionId { get; init; }
}
