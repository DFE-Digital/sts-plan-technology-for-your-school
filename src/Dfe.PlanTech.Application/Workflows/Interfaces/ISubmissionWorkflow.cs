using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Workflows.Interfaces;

public interface ISubmissionWorkflow
{
    Task<SqlSubmissionDto> CloneLatestCompletedSubmission(
        int establishmentId,
        QuestionnaireSectionEntry section
    );
    Task ConfirmCheckAnswersAndUpdateRecommendationsAsync(
        int establishmentId,
        int? matEstablishmentId,
        int submissionId,
        int userId,
        QuestionnaireSectionEntry section
    );
    Task SetSubmissionDeletedAsync(int establishmentId, string sectionId);
    Task SetSubmissionInaccessibleAsync(int submissionId);
    Task SetSubmissionInProgressAsync(int submissionId);
    Task SetSubmissionInaccessibleAsync(int establishmentId, string sectionId);
    Task SetSubmissionInProgressAsync(int establishmentId, string sectionId);
    Task<SqlSubmissionDto?> GetLatestSubmissionWithOrderedResponsesAsync(int establishmentId, QuestionnaireSectionEntry section, SubmissionStatus? status);
    Task<List<SqlSectionStatusDto>> GetSectionStatusesAsync(int establishmentId, IEnumerable<string> sectionIds);
    Task<SqlSectionStatusDto> GetSectionSubmissionStatusAsync(int establishmentId, string sectionId, SubmissionStatus status);
    Task<SqlSubmissionDto> GetSubmissionByIdAsync(int submissionId);
    Task SetLatestSubmissionViewedAsync(int establishmentId, string sectionId);
    Task SetMaturityAndMarkAsReviewedAsync(int submissionId);
    Task SetSubmissionReviewedAsync(int submissionId);
    Task<int> SubmitAnswer(
        int userId,
        int activeEstablishmentId,
        int userEstablishmentId,
        SubmitAnswerModel answerModel
    );
}
