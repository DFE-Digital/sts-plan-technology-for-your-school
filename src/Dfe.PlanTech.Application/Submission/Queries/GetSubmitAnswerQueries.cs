using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Submission.Interface;
using Dfe.PlanTech.Application.Users.Interfaces;

namespace Dfe.PlanTech.Application.Submission.Queries
{
    public class GetSubmitAnswerQueries
    {

        private readonly IGetQuestionQuery _getQuestionQuery;
        private readonly IGetResponseQuery _getResponseQuery;
        private readonly IGetSubmissionQuery _getSubmissionQuery;
        private readonly Application.Questionnaire.Queries.GetQuestionQuery _getQuestionnaireQuery;
        private readonly IUser _user;

        public GetSubmitAnswerQueries(
            IGetQuestionQuery getQuestionQuery,
            IGetResponseQuery getResponseQuery,
            IGetSubmissionQuery getSubmissionQuery,
            Application.Questionnaire.Queries.GetQuestionQuery getQuestionnaireQuery,
            IUser user)
        {
            _getQuestionQuery = getQuestionQuery;
            _getResponseQuery = getResponseQuery;
            _getQuestionnaireQuery = getQuestionnaireQuery;
            _getSubmissionQuery = getSubmissionQuery;
            _user = user;
        }

        public async Task<Domain.Questions.Models.Question?> GetResponseQuestion(int questionId)
        {
            return await _getQuestionQuery.GetQuestionBy(questionId);
        }

        public async Task<Domain.Responses.Models.Response[]?> GetResponseList(int submissionId)
        {
            return await _getResponseQuery.GetResponseListBy(submissionId);
        }

        public async Task<Domain.Submissions.Models.Submission?> GetSubmission(int establishmentId, string sectionId)
        {
            return await _getSubmissionQuery.GetSubmissionBy(establishmentId, sectionId);
        }

        public async Task<Domain.Questionnaire.Models.Question> GetQuestionnaireQuestion(string id, string? section, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
            return await _getQuestionnaireQuery.GetQuestionById(id, section, cancellationToken) ?? throw new KeyNotFoundException($"Could not find question with id {id}");
        }

        public async Task<int> GetUserId()
        {
            return Convert.ToUInt16(await _user.GetCurrentUserId());
        }

        public async Task<int> GetEstablishmentId()
        {
            return Convert.ToUInt16(await _user.GetEstablishmentId());
        }

    }
}