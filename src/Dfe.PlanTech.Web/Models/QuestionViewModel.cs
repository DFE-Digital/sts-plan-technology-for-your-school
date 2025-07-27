using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Web.Models;

public class QuestionViewModel
{
    [Required]
    public CmsQuestionnaireQuestionDto Question { get; init; } = null!;

    public string? AnswerSysId { get; init; }

    public IEnumerable<string>? ErrorMessages { get; set; }

    public string? SectionName { get; init; }

    public string? SectionSlug { get; init; }

    public string? SectionId { get; init; }
}
