using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Workflows.Interfaces
{
    public interface IRecommendationWorkflow
    {
        Task<SqlEstablishmentDto?> GetEstablishmentByReferenceAsync(string establishmentReference);
        Task<IEnumerable<SqlEstablishmentDto>> GetEstablishmentsByReferencesAsync(IEnumerable<string> establishmentReferences);
        Task<List<SqlEstablishmentLinkDto>> GetGroupEstablishments(int establishmentId);
        Task<Dictionary<string, SqlEstablishmentRecommendationHistoryDto>> GetMostRecentRecommendationStatusesAsync(IEnumerable<string> recommendationContentfulReferences, int establishmentId);
        Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(string establishmentUrn, string establishmentName);
        Task<int> RecordGroupSelection(UserGroupSelectionModel userGroupSelectionModel);
    }
}
