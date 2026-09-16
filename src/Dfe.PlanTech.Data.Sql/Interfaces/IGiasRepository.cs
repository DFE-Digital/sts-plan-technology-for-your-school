using Dfe.PlanTech.Core.DataTransferObjects;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IGiasRepository
{
    Task<GiasEstablishmentEntity?> GetSingleAcademySchool(int groupUid);
    Task<GroupEstablishmentDTO?> GetGiasGroupByGroupUIDAsync(int groupUid);
    Task<GiasEstablishmentEntity?> GetSchoolEstablishmentByURN(int urn);
    Task<bool> AreAllSchoolsWithinGroup(int groupUId, IEnumerable<string> urns);

    Task<bool> IsSchoolWithinGroup(int groupUId, string urn);
    Task<bool> IsSchoolWithinGroup(int groupUId, IEnumerable<string> urns);
    Task<List<int>> GetLinkedURNSForGroupAsync(int groupUid);
}
