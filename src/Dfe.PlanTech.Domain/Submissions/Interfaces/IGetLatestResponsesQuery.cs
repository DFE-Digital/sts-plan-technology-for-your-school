using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Submissions.Interfaces;

public interface IGetLatestResponsesQuery
{
    Task<QuestionWithAnswer?> GetLatestResponseForQuestion(int establishmentId, string sectionId, string questionId, CancellationToken cancellationToken = default);

    Task<SubmissionResponsesDto?> GetLatestResponses(int establishmentId, string sectionId, bool completedSubmission, CancellationToken cancellationToken = default);

    Task<DateTime?> GetLatestCompletionDate(int establishmentId, string sectionId, bool completedSubmission, CancellationToken cancellationToken = default);

    Task ViewLatestSubmission(int establishmentId, string sectionId);
}
