using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces
{
    public interface IStoredProcedureRepository
    {
        Task<List<SectionStatusEntity>> GetSectionStatusesAsync(string sectionIds, int establishmentId);

        /// <summary>
        /// NOTE: Despite the method name "HardDelete", this method actually performs a SOFT DELETE.
        /// The underlying stored procedure sets the 'deleted' flag to 1 rather than removing the record entirely.
        /// </summary>
        Task HardDeleteCurrentSubmissionAsync(int establishmentId, string sectionId);

        Task<int> RecordGroupSelection(UserGroupSelectionModel userGroupSelectionModel);
        Task<int> SetMaturityForSubmissionAsync(int submissionId);
        Task<int> SubmitResponse(AssessmentResponseModel response);
    }
}
