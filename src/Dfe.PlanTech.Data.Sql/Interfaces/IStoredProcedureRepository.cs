using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IStoredProcedureRepository
{
    Task<List<SectionStatusEntity>> GetSectionStatusesAsync(string sectionIds, int establishmentId);

    Task SetSubmissionDeletedAsync(int establishmentId, string sectionId);

    Task<int> RecordGroupSelection(UserGroupSelectionModel userGroupSelectionModel);
    Task<int> SetMaturityForSubmissionAsync(int submissionId);
    Task<int> SubmitResponse(AssessmentResponseModel response);
}
