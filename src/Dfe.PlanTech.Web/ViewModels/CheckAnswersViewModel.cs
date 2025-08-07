using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Web.ViewModels;

public class CheckAnswersViewModel
{
    [Required]
    public CmsComponentTitleDto Title { get; init; } = null!;

    [Required]
    public string SectionName { get; init; } = null!;

    [Required]
    public SubmissionResponsesViewModel SubmissionResponses { get; init; } = null!;

    [Required]
    public List<CmsEntryDto> Content { get; init; } = null!;

    public string? CategorySlug { get; init; }
    public int? SubmissionId { get; init; }
    public string? SectionSlug { get; init; } = null;
    public string? Slug { get; init; } = null;
    public string? ErrorMessage { get; set; } = null;
}
