using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class SubmissionResponseViewModel
{
    [Required]
    public string QuestionRef { get; init; } = null!;

    [Required]
    public string? QuestionSlug { get; init; } = string.Empty;

    [Required]
    public string? QuestionText { get; init; } = string.Empty;

    [Required]
    public string AnswerRef { get; init; } = null!;

    [Required]
    public string? AnswerText { get; init; } = "";

    [Required]
    public DateTime? DateCreated { get; init; } = null!;

    public SubmissionResponseViewModel(QuestionWithAnswerModel response)
    {
        QuestionRef = response.QuestionSysId;
        QuestionSlug = response.QuestionSlug;
        QuestionText = response.QuestionText;
        AnswerRef = response.AnswerSysId;
        AnswerText = response.AnswerText;
        DateCreated = response.DateCreated;
    }
}
