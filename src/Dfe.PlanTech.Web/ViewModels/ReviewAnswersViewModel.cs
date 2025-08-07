using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Web.ViewModels;

public class ReviewAnswersViewModel
{
    [Required]
    public string SectionName { get; init; } = null!;

    [Required]
    public SubmissionResponsesViewModel? SubmissionResponses { get; init; } = null!;

    [Required]
    public CmsComponentTitleDto Title { get; init; } = null!;

    public List<CmsEntryDto>? Content { get; init; }
    public string? CategorySlug { get; init; } = null;
    public string? ErrorMessage { get; set; } = null;
    public string? SectionSlug { get; init; } = null;
    public string? Slug { get; init; } = null;
    public int? SubmissionId { get; init; }
}
