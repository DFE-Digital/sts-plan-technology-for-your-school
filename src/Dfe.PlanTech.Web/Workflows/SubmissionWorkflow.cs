using Dfe.PlanTech.Core.DataTransferObjects;
using Dfe.PlanTech.Infrastructure.Data.Sql.Repositories;

namespace Dfe.PlanTech.Web.Workflows
{
    public class SubmissionWorkflow
    {
        private readonly SubmissionRepository _submissionRepository;

        public SubmissionWorkflow(
            SubmissionRepository submissionRepository
        )
        {
            _submissionRepository = submissionRepository;
        }

        public async Task<SqlSubmissionDto?> GetLatestCompletedSubmission(int establishmentId, string sectionId)
        {
            var currentSubmission = await _submissionRepository.GetLatestSubmissionAsync(establishmentId, sectionId, isCompleted: true);
            return currentSubmission?.ToDto();
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

        public Task DeleteCurrentSubmission(int establishmentId, string sectionId)
        {
            return _submissionRepository.SetSubmissionInaccessibleAsync(establishmentId, sectionId);
        }
    }
}
