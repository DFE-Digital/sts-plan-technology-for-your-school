using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModels;

namespace Dfe.PlanTech.Application.Services.Interfaces;

public interface ISubmissionService
{
    Task ConfirmCheckAnswersAndUpdateRecommendationsAsync(int establishmentId, int? matEstablishmentId, int submissionId, int userId, QuestionnaireSectionEntry section);
    Task ConfirmCheckAnswersAsync(int submissionId);
    Task SetSubmissionInaccessibleAsync(int establishmentId, string sectionId);
    Task<Dictionary<string, SqlEstablishmentRecommendationHistoryDto>> GetLatestRecommendationStatusesByRecommendationIdAsync(int establishmentId);
    Task RestoreInaccessibleSubmissionAsync(int establishmentId, string sectionId);
    Task<SubmissionResponsesModel?> GetLatestSubmissionResponsesModel(int establishmentId, QuestionnaireSectionEntry section, bool isCompletedSubmission);
    Task<List<SqlSectionStatusDto>> GetSectionStatusesForSchoolAsync(int establishmentId, IEnumerable<string> sectionIds);
    Task<SqlSubmissionDto> GetSubmissionByIdAsync(int submissionId);
    Task<SubmissionRoutingDataModel> GetSubmissionRoutingDataAsync(int establishmentId, QuestionnaireSectionEntry section, bool? isCompletedSubmission);
    Task<SqlSubmissionDto> RemovePreviousSubmissionsAndCloneMostRecentCompletedAsync(int establishmentId, QuestionnaireSectionEntry section);
    Task SetLatestSubmissionViewedAsync(int establishmentId, string sectionId);
    Task<int> SubmitAnswerAsync(int userId, int establishmentId, int? matEstablishmentId, SubmitAnswerModel answerModel);
}
