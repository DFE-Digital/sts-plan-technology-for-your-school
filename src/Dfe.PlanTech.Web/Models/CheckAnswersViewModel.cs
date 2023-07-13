using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Domain.Questionnaire.Models;
namespace Dfe.PlanTech.Web.Models;

public class CheckAnswersViewModel : BaseViewModel
{
    [Required]
    public CheckAnswerDto CheckAnswerDto { get; init; } = null!;

    public int? SubmissionId { get; init; }
}