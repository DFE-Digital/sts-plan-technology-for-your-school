using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;

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
        IEnumerable<SubmissionStatus> statuses
    );

    Task<SubmissionEntity?> GetLatestCompletedSubmissionBySectionIdAsync(
        int establishmentId,
        string sectionId
    );

    Task<SubmissionEntity?> GetSubmissionByIdAsync(int submissionId);

    Task<SubmissionEntity> SetSubmissionInaccessibleAsync(int submissionId);

    Task<SubmissionEntity> SetSubmissionInProgressAsync(int submissionId);

    Task SetSubmissionInaccessibleAsync(int establishmentId, string sectionId);

    Task SetSubmissionInProgressAsync(int establishmentId, string sectionId);

    Task<SubmissionEntity> SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(
        int submissionId
    );

    Task<List<SectionStatusEntity>> GetSectionStatusesAsync(string sectionIds, int establishmentId);

    Task SetSubmissionDeletedAsync(int establishmentId, string sectionId);

    Task<int> SubmitResponse(AssessmentResponseModel response);

    /// <summary>
    /// Optional status filter, latest per SECTION across PER school of those passed in
    /// </summary>
    /// <param name="establishmentIds"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    Task<List<SubmissionEntity>> GetLatestEstablishmentsSubmissionsByEstablishmentAndSectionAsync(
        IEnumerable<int> establishmentIds, SubmissionStatus status = SubmissionStatus.CompleteReviewed
    );

    /// <summary>
    ///  Optional status filter, Count of submissions per SECTION across ALL schools passed in (using URN)
    /// </summary>
    /// <param name="urns">List of establishment refs to include</param>
    /// <param name="status"></param>
    /// <returns>Dictionary<string, int></returns>
    Task<Dictionary<string, int>> GetSubmissionsCountBySectionAsync(IEnumerable<string> urns, SubmissionStatus status = SubmissionStatus.CompleteReviewed);

    /// <summary>
    /// Optional status filter, Count of submissions per SECTION across ALL schools passed in (using dboEstId list of schools)
    /// </summary>
    /// <param name="dboSchoolIds">List of establishment ids to include</param>
    /// <param name="status"></param>
    /// <returns></returns>
    Task<Dictionary<string, int>> GetSubmissionsCountBySectionAsync(IEnumerable<int> dboSchoolIds, SubmissionStatus status = SubmissionStatus.CompleteReviewed);

    /// <summary>
    /// Filter by section; latest per school
    /// </summary>
    /// <param name="establishmentIds"></param>
    /// <param name="sectionId"></param>
    /// <returns></returns>
    Task<List<SubmissionEntity>> GetLatestSubmissionPerEstablishmentForSectionAsync(
        IEnumerable<int> establishmentIds,
        string sectionId
    );
}
