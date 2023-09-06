using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web.Models;

public class CheckAnswerRowViewModel
{
  public int? SubmissionId { get; init; }

  public required QuestionWithAnswer QuestionWithAnswer { get; init; }
}