using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Web.Models;

public class CheckAnswersViewModel : BaseViewModel
{

    [Required]
    public Title Title { get; init; } = null!;

    [Required]
    public string SectionName { get; init; } = null!;

    [Required]
    public CheckAnswerDto CheckAnswerDto { get; init; } = null!;

    [Required]
    public IContentComponent[] Content { get; init; } = null!;

    public int? SubmissionId { get; init; }
}