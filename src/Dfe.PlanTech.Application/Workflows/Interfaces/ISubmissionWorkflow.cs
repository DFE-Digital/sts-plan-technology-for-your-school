using System.Threading.Tasks;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Workflows.Interfaces;

public interface ISubmissionWorkflow
{
    Task<SqlSubmissionDto> CloneLatestCompletedSubmission(int establishmentId, QuestionnaireSectionEntry section);
    Task ConfirmCheckAnswersAndUpdateRecommendationsAsync(int establishmentId, int? matEstablishmentId, int userId, QuestionnaireSectionEntry section);
    Task DeleteSubmissionHardAsync(int establishmentId, string sectionId);
    Task DeleteSubmissionSoftAsync(int submissionId);
    Task DeleteSubmissionSoftAsync(int establishmentId, string sectionId);
    Task<SqlSubmissionDto?> GetLatestSubmissionWithOrderedResponsesAsync(int establishmentId, QuestionnaireSectionEntry section, bool? isCompletedSubmission);
    Task<List<SqlSectionStatusDto>> GetSectionStatusesAsync(int establishmentId, IEnumerable<string> sectionIds);
    Task<SqlSectionStatusDto> GetSectionSubmissionStatusAsync(int establishmentId, string sectionId, bool isCompleted);
    Task<SqlSubmissionDto> GetSubmissionByIdAsync(int submissionId);
    Task SetLatestSubmissionViewedAsync(int establishmentId, string sectionId);
    Task SetMaturityAndMarkAsReviewedAsync(int submissionId);
    Task SetSubmissionReviewedAsync(int submissionId);
    Task<int> SubmitAnswer(int userId, int establishmentId, SubmitAnswerModel answerModel);
}
