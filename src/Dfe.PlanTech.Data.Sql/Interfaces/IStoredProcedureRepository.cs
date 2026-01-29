using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IStoredProcedureRepository
{
    Task<int> SetMaturityForSubmissionAsync(int submissionId);
    Task<int> SubmitResponse(AssessmentResponseModel response);
}
