using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class QuestionViewModel
{
    [Required]
    public required QuestionnaireQuestionEntry Question { get; init; }

    public string? AnswerSysId { get; init; }
    public string? CategorySlug { get; init; }
    public IEnumerable<string>? ErrorMessages { get; set; }
    public string? SectionName { get; init; }
    public string? SectionSlug { get; init; }
    public string? SectionId { get; init; }
}
