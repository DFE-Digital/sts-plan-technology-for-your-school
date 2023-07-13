using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web.Models;

public class CheckAnswersViewModel : BaseViewModel
{
    [Required]
    public Question[] Questions { get; init; } = Array.Empty<Question>();

    public int? SubmissionId { get; init; }

    [Required]
    public Answer[] Answers { get; init; } = Array.Empty<Answer>();
}