using System.Security.Authentication;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.RoutingDataModel;
using Dfe.PlanTech.Data.Sql.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Application.Workflows
{
    public class SubmissionWorkflow(
        StoredProcedureRepository storedProcedureRepository,
        SubmissionRepository submissionRepository
    )
    {
        private readonly StoredProcedureRepository _storedProcedureRepository = storedProcedureRepository ?? throw new ArgumentNullException(nameof(storedProcedureRepository));
        private readonly SubmissionRepository _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));

        public async Task CloneLatestCompletedSubmission(int establishmentId, string sectionId)
        {
            var submissionWithResponses = await _submissionRepository.GetLatestSubmissionAndResponsesAsync(establishmentId, sectionId, isCompleted: false);
            await _submissionRepository.CloneSubmission(submissionWithResponses);
        }

        // On the action on the controller, we should redirect to a new route called "GetNextUnansweredQuestionForSection"
        // which will then either redirect to the "GetQuestionBySlug" route or "Check Answers" route
        public async Task<int> SubmitAnswer(SubmitAnswerViewModel questionAnswer, CancellationToken cancellationToken = default)
        {
            if (questionAnswer?.ChosenAnswer is null)
            {
                throw new InvalidDataException($"{nameof(questionAnswer.ChosenAnswer)} is null");
            }

            int userId = _currentUser.UserId ?? throw new AuthenticationException("User is not authenticated");
            int establishmentId = _currentUser.EstablishmentId ?? throw new AuthenticationException($"User has no {_currentUser.EstablishmentId}");

            var model = new AssessmentResponseModel(userId, establishmentId, questionAnswer.ToModel());
            var responseId = await _storedProcedureRepository.SubmitResponse(model);

            return responseId;
        }

        public async Task<List<SqlSectionStatusDto>> GetSectionStatusesAsync(int establishmentId, IEnumerable<string> sectionIds)
        {
            var sectionIdsInput = string.Join(',', sectionIds);
            var statuses = await _storedProcedureRepository.GetSectionStatusesAsync(sectionIdsInput, establishmentId);
            return statuses.Select(s => s.AsDto()).ToList();
        }

        public async Task<SqlSubmissionDto?> GetInProgressSubmissionAsync(int establishmentId, string sectionId)
        {
            var currentSubmission = await _submissionRepository.GetLatestSubmissionAsync(establishmentId, sectionId, isCompleted: false);
            return currentSubmission?.AsDto();
        }

        public async Task<SqlSectionStatusDto> GetSectionSubmissionStatusAsync(int establishmentId, string sectionId, bool isCompleted)
        {
            var latestSubmission = await _submissionRepository
                .GetPreviousSubmissionsInDescendingOrder(establishmentId, sectionId, isCompleted)
                .FirstOrDefaultAsync();

            if (latestSubmission is not null)
            {
                return new SqlSectionStatusDto
                {
                    Completed = latestSubmission.Completed,
                    LastMaturity = latestSubmission.Maturity,
                    SectionId = latestSubmission.SectionId,
                    Status = latestSubmission.Completed ? SubmissionStatus.CompleteReviewed : SubmissionStatus.InProgress
                };
            }

            return new SqlSectionStatusDto
            {
                Completed = false,
                SectionId = sectionId,
                Status = SubmissionStatus.NotStarted
            };
        }

        public async Task SetMaturityAndMarkAsReviewedAsync(int submissionId)
        {
            await _storedProcedureRepository.SetMaturityForSubmissionAsync(submissionId);
            await _submissionRepository.SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(submissionId);
        }

        public async Task SetLatestSubmissionViewedAsync(int establishmentId, string sectionId)
        {
            await _submissionRepository.SetLatestSubmissionViewedAsync(establishmentId, sectionId);
        }

        public async Task SetSubmissionReviewedAsync(int submissionId)
        {
            var submission = await _submissionRepository.SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(submissionId);
            await _submissionRepository.SetPreviousCompletedReviewedSubmissionsInaccessible(submission);
        }

        public Task SetSubmissionInaccessibleAsync(int establishmentId, string sectionId)
        {
            return _submissionRepository.SetSubmissionInaccessibleAsync(establishmentId, sectionId);
        }

        public Task SetSubmissionInaccessibleAsync(int submissionId)
        {
            return _submissionRepository.SetSubmissionInaccessibleAsync(submissionId);
        }
    }
}
