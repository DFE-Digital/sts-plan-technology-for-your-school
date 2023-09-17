using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Responses.Interface
{
    public interface IGetLatestResponseListForSubmissionQuery
    {
        Task<QuestionWithAnswer?> GetLatestResponseForQuestion(int establishmentId, string sectionId, string questionId, CancellationToken cancellationToken = default);

        Task<SubmissionWithResponses> GetLatestResponses(int establishmentId, string sectionId, CancellationToken cancellationToken = default);
    }
}