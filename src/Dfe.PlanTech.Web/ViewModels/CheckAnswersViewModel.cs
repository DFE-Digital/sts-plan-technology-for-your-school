using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewModels;

public class CheckAnswersViewModel
{
    [Required]
    public ComponentTitleEntry Title { get; init; } = null!;

    [Required]
    public string SectionName { get; init; } = null!;

    [Required]
    public SubmissionResponsesViewModel SubmissionResponses { get; init; } = null!;

    [Required]
    public List<ContentfulEntry> Content { get; init; } = null!;

    public string? CategorySlug { get; init; }
    public int? SubmissionId { get; init; }
    public string? SectionSlug { get; init; } = null;
    public string? Slug { get; init; } = null;
    public string? ErrorMessage { get; set; } = null;
}
