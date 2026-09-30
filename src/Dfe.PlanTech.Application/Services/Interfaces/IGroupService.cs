using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Application.Services.Interfaces;

public interface IGroupService
{
    Task<List<SqlSubmissionDto>> GetGroupCompletedSubmissionsBySections(int[] establishmentIds);
    Task<List<SubmissionInformationModel>> GetGroupSubmissionInformationForSection(List<SqlEstablishmentLinkDto> establishmentLinks, string sectionId);

    Task<List<(EstablishmentEntity establishment, EstablishmentRecommendationHistoryEntity? recommendationHistory)>>
        GetLatestGroupEstablishmentRecommendationHistoryByRecommendationId(int establishmentId, int recommendationId);
}
