using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Submissions.Interfaces;

public interface ISubmitAnswerCommand
{
    public Task<int> SubmitAnswer(SubmitAnswerDto submitAnswerDto, CancellationToken cancellationToken = default);
}
