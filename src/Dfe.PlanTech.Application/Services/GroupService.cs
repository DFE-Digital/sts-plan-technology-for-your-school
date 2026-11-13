using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Application.Services;

public class GroupService(IGroupWorkflow groupWorkflow) : IGroupService
{
    private readonly IGroupWorkflow _groupWorkflow =
        groupWorkflow ?? throw new ArgumentNullException(nameof(groupWorkflow));

    public Task<List<SqlSubmissionDto>> GetGroupCompletedSubmissionsBySections(
        int[] establishmentIds
    )
    {
        return _groupWorkflow.GetGroupCompletedSubmissions(establishmentIds);
    }

    public Task<List<SubmissionInformationModel>> GetGroupSubmissionInformationForSection(
        List<SelectedEstablishmentDataModel> groupSchools,
        string sectionId
    )
    {
        return _groupWorkflow.GetGroupSubmissionInformationForSection(groupSchools, sectionId);
    }

    public Task<
        List<(
            EstablishmentEntity establishment,
            EstablishmentRecommendationHistoryEntity? recommendationHistory
        )>
    > GetLatestGroupEstablishmentRecommendationHistoryByRecommendationId(
        int establishmentId,
        int recommendationId
    )
    {
        return _groupWorkflow.GetLatestGroupEstablishmentRecommendationHistoryByRecommendationId(
            establishmentId,
            recommendationId
        );
    }
}
