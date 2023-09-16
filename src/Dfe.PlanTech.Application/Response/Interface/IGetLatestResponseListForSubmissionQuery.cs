using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Response.Interface
{
    public interface IGetLatestResponseListForSubmissionQuery
    {
        Task<List<QuestionWithAnswer>> GetLatestResponseListForSubmissionBy(int submissionId);

        Task<List<QuestionWithAnswer>> GetResponseListByDateCreated(int submissionId);

        Task<QuestionWithAnswer?> GetLatestResponse(int establishmentId, string sectionId);

        Task<QuestionWithAnswer?> GetLatestResponseForQuestion(int establishmentId, string sectionId, string questionId);

        Task<SubmissionWithResponses> GetLatestResponses(int establishmentId, string sectionId);
    }
}