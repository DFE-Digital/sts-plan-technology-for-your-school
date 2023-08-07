using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Response.Interface
{
    public interface IGetLatestResponseListForSubmissionQuery
    {
        Task<List<QuestionWithAnswer>> GetLatestResponseListForSubmissionBy(int submissionId);

        Task<List<QuestionWithAnswer>> GetResponseListByDateCreated(int submissionId);
    }
}