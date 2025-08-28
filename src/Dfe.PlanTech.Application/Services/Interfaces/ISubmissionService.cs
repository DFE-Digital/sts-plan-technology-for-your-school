using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModels;

namespace Dfe.PlanTech.Application.Services.Interfaces
{
    public interface ISubmissionService
    {
        Task ConfirmCheckAnswersAsync(int submissionId);
        Task DeleteCurrentSubmissionHardAsync(int establishmentId, string sectionId);
        Task DeleteCurrentSubmissionSoftAsync(int establishmentId, string sectionId);
        Task<SubmissionResponsesModel?> GetLatestSubmissionResponsesModel(int establishmentId, QuestionnaireSectionEntry section, bool isCompletedSubmission);
        Task<List<SqlSectionStatusDto>> GetSectionStatusesForSchoolAsync(int establishmentId, IEnumerable<string> sectionIds);
        Task<SubmissionRoutingDataModel> GetSubmissionRoutingDataAsync(int establishmentId, QuestionnaireSectionEntry section, bool? isCompletedSubmission);
        Task<SqlSubmissionDto> RemovePreviousSubmissionsAndCloneMostRecentCompletedAsync(int establishmentId, QuestionnaireSectionEntry section);
        Task SetLatestSubmissionViewedAsync(int establishmentId, string sectionId);
        Task<int> SubmitAnswerAsync(int userId, int establishmentId, SubmitAnswerModel answerModel);
    }
}
