using Dfe.PlanTech.Core.Models;

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
