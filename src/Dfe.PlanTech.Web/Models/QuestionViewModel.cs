using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web.Models;

public class QuestionViewModel : BaseViewModel
{
    public required Question Question { get; init; }

    public string? Params { get; init; } = null!;

    public int? SubmissionId { get; init; }
}
