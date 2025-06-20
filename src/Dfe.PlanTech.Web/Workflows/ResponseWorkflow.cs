using Dfe.PlanTech.Core.DataTransferObjects;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Infrastructure.Data.Sql.Entities;
using Dfe.PlanTech.Infrastructure.Data.Sql.Repositories;
using Dfe.PlanTech.Web.Workflows.WorkflowModels;
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

        public async Task<QuestionWithAnswerModel?> GetLatestResponseForQuestion(
            int establishmentId,
            string sectionId,
            string questionId
        )
        {
            return await GetPreviousSubmissions(establishmentId, sectionId, isCompletedSubmission: false, includeRelationships: true)
                .SelectMany(submission => submission.Responses)
                .Where(response => string.Equals(questionId, response.Question.ContentfulRef))
                .OrderByDescending(response => response.DateCreated)
                .Select(response => new QuestionWithAnswerModel(response))
                .FirstOrDefaultAsync();
        }

        public async Task<SubmissionResponsesModel?> GetLatestResponses(int establishmentId, string sectionId, bool isCompletedSubmission)
        {
            var latestSubmission = await GetPreviousSubmissions(establishmentId, sectionId, isCompletedSubmission, includeRelationships: true)
                .Select(submission => new SubmissionResponsesModel(submission))
                .FirstOrDefaultAsync();

            return latestSubmission is not null && latestSubmission.HasResponses
                ? latestSubmission
                : null;
        }

        public async Task ViewLatestSubmission(int establishmentId, string sectionId)
        {
            var currentSubmission = await GetPreviousSubmissions(establishmentId, sectionId, isCompletedSubmission: true)
                .FirstOrDefaultAsync();

            if (currentSubmission is not null)
            {
                await _submissionRepository.SetSubmissionViewedAsync(currentSubmission);
            }
        }

        public async Task<SqlSubmissionDto?> GetLatestCompletedSubmission(int establishmentId, string sectionId)
        {
            var currentSubmission = await GetPreviousSubmissions(establishmentId, sectionId, isCompletedSubmission: true)
                .FirstOrDefaultAsync();

            if (currentSubmission is not null)
            {
                currentSubmission = await _submissionRepository.GetSubmissionByIdAsync(currentSubmission.Id);
            }

            return currentSubmission?.ToDto();
        }

        public async Task<SqlSubmissionDto?> GetInProgressSubmission(int establishmentId, string sectionId)
        {
            // Get latest matching submission
            var submission = await GetPreviousSubmissions(establishmentId, sectionId, isCompletedSubmission: false, includeRelationships: true)
                .OrderByDescending(submission => submission.DateCreated)
                .FirstOrDefaultAsync(submission => string.Equals(submission.Status, SubmissionStatus.InProgress.ToString()));

            if (submission is null)
            {
                return null;
            }

            var latestResponses = submission.Responses
                .GroupBy(response => response.QuestionId)
                .Select(group => group
                    .Where(response => response.DateCreated == group.Max(r => r.DateCreated))
                    .First());

            submission.Responses = latestResponses.ToList();

            return submission.ToDto();
        }

        private IQueryable<SubmissionEntity> GetPreviousSubmissions(
            int establishmentId,
            string sectionId,
            bool isCompletedSubmission,
            bool includeRelationships = false
        )
        {
            return _submissionRepository
                .GetSubmissionsBy(submission =>
                    !submission.Deleted &&
                    submission.Completed == isCompletedSubmission &&
                    submission.EstablishmentId == establishmentId &&
                    submission.SectionId == sectionId,
                    includeRelationships)
                .OrderByDescending(submission => submission.DateCreated);
        }
    }
}
