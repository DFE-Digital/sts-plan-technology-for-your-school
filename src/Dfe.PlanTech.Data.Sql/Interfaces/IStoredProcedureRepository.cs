using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IStoredProcedureRepository
{
    Task<FirstActivityForEstablishmentRecommendationEntity> GetFirstActivityForEstablishmentRecommendationAsync(
        int establishmentId,
        string recommendationContentfulReference
    );
    Task<int> SetMaturityForSubmissionAsync(int submissionId);
    Task<int> SubmitResponse(AssessmentResponseModel response);
}
