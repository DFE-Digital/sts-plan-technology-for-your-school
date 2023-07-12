using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web.Models;

public class CheckYourAnswersViewModel : BaseViewModel
{
    [Required]
    public Question[] Questions { get; init; } = Array.Empty<Question>();
}