using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Application.Workflows.Interfaces;

public interface IGroupWorkflow
{
    Task<List<SqlSubmissionDto>> GetGroupCompletedSubmissions(int[] establishmentIds);
    Task<List<SubmissionInformationModel>> GetGroupSubmissionInformationForSection(
        List<SelectedEstablishmentDataModel> groupSchools,
        string sectionId
    );

    Task<
        List<(
            EstablishmentEntity establishment,
            EstablishmentRecommendationHistoryEntity? recommendationHistory
        )>
    > GetLatestGroupEstablishmentRecommendationHistoryByRecommendationRefAsync(
        int establishmentId,
        string recommendationRef
    );
}
