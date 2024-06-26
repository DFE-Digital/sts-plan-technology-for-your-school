using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Responses.Interfaces;

public interface IGetLatestResponsesQuery
{
    Task<QuestionWithAnswer?> GetLatestResponseForQuestion(int establishmentId, string sectionId, string questionId, CancellationToken cancellationToken = default);

    Task<ResponsesForSubmissionDto?> GetLatestResponses(int establishmentId, string sectionId, bool completedSubmission, CancellationToken cancellationToken = default);
}
