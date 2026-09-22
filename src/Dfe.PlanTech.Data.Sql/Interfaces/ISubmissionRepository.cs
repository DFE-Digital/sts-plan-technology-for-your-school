using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface ISubmissionRepository
{
    Task<SubmissionEntity> CloneSubmission(SubmissionEntity? existingSubmission);

    Task<SubmissionEntity?> GetLatestSubmissionAndResponsesAsync(
        int establishmentId,
        string sectionId,
        SubmissionStatus? status
    );

    Task<SubmissionEntity?> GetLatestSubmissionAndResponsesAsync(
        int establishmentId,
        string sectionId,
        IEnumerable<SubmissionStatus> statuses
    );

    Task<SubmissionEntity?> GetLatestCompletedSubmissionBySectionIdAsync(
        int establishmentId,
        string sectionId
    );

    Task<SubmissionEntity?> GetSubmissionByIdAsync(int submissionId);

    Task<SubmissionEntity?> GetSubmissionByIdWithResponsesAsync(int submissionId);

    Task<List<SectionStatusEntity>> GetSectionStatusesAsync(string sectionIds, int establishmentId);

    Task SetSubmissionDeletedAsync(int establishmentId, string sectionId);

    Task<List<SubmissionEntity>> GetLatestEstablishmentsCompletedSubmissionsBySectionsAsync(
        IEnumerable<int> establishmentIds
    );

    Task<List<SubmissionEntity>> GetLatestSubmissionPerEstablishmentForSectionAsync(
        IEnumerable<int> establishmentIds,
        string sectionId
    );

    Task<int> SelectOrInsertSubmissionAsync(
        string sectionId,
        string sectionName,
        int establishmentId
    );

    Task<SubmissionEntity> SetSubmissionInaccessibleAsync(int submissionId);

    Task<SubmissionEntity> SetSubmissionInProgressAsync(int submissionId);

    Task SetSubmissionInaccessibleAsync(int establishmentId, string sectionId);

    Task SetSubmissionInProgressAsync(int establishmentId, string sectionId);

    Task<SubmissionEntity> SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(
        int submissionId
    );

    Task UpdateSubmissionDatesAsync(int submissionId, Guid userActionId);
}
