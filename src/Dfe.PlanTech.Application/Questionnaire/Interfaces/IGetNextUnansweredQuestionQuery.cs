using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Questionnaire.Interfaces;

public interface IGetNextUnansweredQuestionQuery{
  public Task<Question?> GetNextUnansweredQuestion(int establishmentId, Section section, CancellationToken cancellationToken = default);
}
