using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces
{
    public interface ISubmissionRepository
    {
        Task<SubmissionEntity> CloneSubmission(SubmissionEntity? existingSubmission);
        Task DeleteCurrentSubmission(int establishmentId, int sectionId);
        Task<SubmissionEntity?> GetLatestSubmissionAndResponsesAsync(int establishmentId, string sectionId, bool? isCompletedSubmission);
        Task<SubmissionEntity?> GetSubmissionByIdAsync(int submissionId);
        Task SetLatestSubmissionViewedAsync(int establishmentId, string sectionId);
        Task<SubmissionEntity> SetSubmissionInaccessibleAsync(int submissionId);
        Task SetSubmissionInaccessibleAsync(int establishmentId, string sectionId);
        Task<SubmissionEntity> SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(int submissionId);
    }
}