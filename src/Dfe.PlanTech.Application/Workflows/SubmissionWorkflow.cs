using System.Security.Authentication;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.RoutingDataModel;
using Dfe.PlanTech.Infrastructure.Data.Sql.Repositories;
using Dfe.PlanTech.Application.Context;
using Dfe.PlanTech.Application.ViewModels;
using Microsoft.EntityFrameworkCore;
using Dfe.PlanTech.Data.Sql.Repositories;

namespace Dfe.PlanTech.Application.Workflows
{
    public class SubmissionWorkflow(
        CurrentUser currentUser,
        StoredProcedureRepository storedProcedureRepository,
        SubmissionRepository submissionRepository
    )
    {
        private readonly CurrentUser _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
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

        public async Task SetLatestSubmissionViewedAsync(int establishmentId, string sectionId)
        {
            await _submissionRepository.SetLatestSubmissionViewedAsync(establishmentId, sectionId);
        }

        public async Task SetSubmissionReviewedAsync(int submissionId)
        {
            var submission = await _submissionRepository.SetSubmissionReviewedAsync(submissionId);
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
