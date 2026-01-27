using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;
using System.Threading.Tasks;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface ISubmissionRepository
{
    Task<SubmissionEntity> CloneSubmission(SubmissionEntity? existingSubmission);
    Task ConfirmCheckAnswersAndUpdateRecommendationsAsync(
        int establishmentId,
        int? matEstablishmentId,
        int submissionId,
        int userId,
        QuestionnaireSectionEntry section
    );
    Task<SubmissionEntity?> GetLatestSubmissionAndResponsesAsync(
        int establishmentId,
        string sectionId,
        SubmissionStatus? status
    );
    Task<SubmissionEntity?> GetSubmissionByIdAsync(int submissionId);
    Task SetLatestSubmissionViewedAsync(int establishmentId, string sectionId);
    Task<SubmissionEntity> SetSubmissionInaccessibleAsync(int submissionId);
    Task<SubmissionEntity> SetSubmissionInProgressAsync(int submissionId);
    Task SetSubmissionInaccessibleAsync(int establishmentId, string sectionId);
    Task SetSubmissionInProgressAsync(int establishmentId, string sectionId);
    Task<SubmissionEntity> SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(
      int submissionId
    );
    Task<List<SectionStatusEntity>> GetSectionStatusesAsync(string sectionIds, int establishmentId);
    Task SetSubmissionDeletedAsync(int establishmentId, string sectionId);
}
