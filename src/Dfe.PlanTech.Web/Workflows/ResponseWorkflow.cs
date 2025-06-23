using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Infrastructure.Data.Sql.Repositories;
using Dfe.PlanTech.Web.Workflows.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Web.Workflows
{
    public class ResponseWorkflow
    {
        private readonly ResponseRepository _responseRepository;
        private readonly SubmissionRepository _submissionRepository;

        public ResponseWorkflow(
            ResponseRepository responseRepository,
            SubmissionRepository submissionRepository
        )
        {
            _responseRepository = responseRepository;
            _submissionRepository = submissionRepository;
        }

        public async Task<QuestionWithAnswerModel?> GetLatestResponseForQuestion(int establishmentId, string sectionId, string questionId)
        {
            return await _submissionRepository.GetPreviousSubmissions(establishmentId, sectionId, isCompleted: false, includeRelationships: true)
                .SelectMany(submission => submission.Responses)
                .Where(response => string.Equals(questionId, response.Question.ContentfulRef))
                .OrderByDescending(response => response.DateCreated)
                .Select(response => new QuestionWithAnswerModel(response))
                .FirstOrDefaultAsync();
        }

        public async Task<SubmissionResponsesModel?> GetLatestResponses(int establishmentId, string sectionId, bool isCompletedSubmission)
        {
            var latestSubmission = await _submissionRepository.GetPreviousSubmissions(establishmentId, sectionId, isCompletedSubmission, includeRelationships: true)
                .Select(submission => new SubmissionResponsesModel(submission))
                .FirstOrDefaultAsync();

            return latestSubmission is not null && latestSubmission.HasResponses
                ? latestSubmission
                : null;
        }
    }
}
