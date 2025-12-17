using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface ISubmissionRepository
{
    Task<SubmissionEntity> CloneSubmission(SubmissionEntity? existingSubmission);
    Task ConfirmCheckAnswersAndUpdateRecommendationsAsync(int establishmentId, int? matEstablishmentId, int submissionId, int userId, QuestionnaireSectionEntry section);
    Task<SubmissionEntity?> GetLatestSubmissionAndResponsesAsync(int establishmentId, string sectionId, bool? isCompletedSubmission);
    Task<SubmissionEntity?> GetSubmissionByIdAsync(int submissionId);
    Task SetLatestSubmissionViewedAsync(int establishmentId, string sectionId);
    Task<SubmissionEntity> SetSubmissionInaccessibleAsync(int submissionId);
    Task<SubmissionEntity> SetSubmissionInProgressAsync(int submissionId);
    Task SetSubmissionInaccessibleAsync(int establishmentId, string sectionId);
    Task SetSubmissionInProgressAsync(int establishmentId, string sectionId);
    Task<SubmissionEntity> SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(int submissionId);
}
