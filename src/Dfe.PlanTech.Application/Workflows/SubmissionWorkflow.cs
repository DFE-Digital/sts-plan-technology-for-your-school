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

        public async Task<SqlSubmissionDto?> GetLatestSubmissionAsync(int establishmentId, string sectionId, bool isCompletedSubmission, bool includeResponses)
        {
            var latestSubmission = await _submissionRepository.GetPreviousSubmissionsInDescendingOrder(
                establishmentId,
                sectionId,
                isCompletedSubmission,
                includeResponses)
                .FirstOrDefaultAsync();

            return latestSubmission?.AsDto();
        }

        // On the action on the controller, we should redirect to a new route called "GetNextUnansweredQuestionForSection"
        // which will then either redirect to the "GetQuestionBySlug" route or "Check Answers" route
        public async Task<int> SubmitAnswer(int userId, int establishmentId, SubmitAnswerModel answerModel)
        {
            if (answerModel is null)
            {
                throw new InvalidDataException($"{nameof(answerModel)} is null");
            }

            var model = new AssessmentResponseModel(userId, establishmentId, answerModel);
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
            await _submissionRepository.SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(submissionId);
        }

        public Task SetSubmissionInaccessibleAsync(int establishmentId, string sectionId)
        {
            return _submissionRepository.SetSubmissionInaccessibleAsync(establishmentId, sectionId);
        }

        public Task SetSubmissionInaccessibleAsync(int submissionId)
        {
            return _submissionRepository.SetSubmissionInaccessibleAsync(submissionId);
        }

        public Task HardDeleteSubmissionAsync(int establishmentId, string sectionId)
        {
            return _storedProcedureRepository.HardDeleteCurrentSubmissionAsync(establishmentId, sectionId);
        }
    }
}
