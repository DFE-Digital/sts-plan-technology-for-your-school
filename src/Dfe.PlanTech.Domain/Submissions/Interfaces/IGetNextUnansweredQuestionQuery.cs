using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Submissions.Interfaces;

public interface IGetNextUnansweredQuestionQuery
{
    public Task<Question?> GetNextUnansweredQuestion(int establishmentId, Section section, CancellationToken cancellationToken = default);
}
