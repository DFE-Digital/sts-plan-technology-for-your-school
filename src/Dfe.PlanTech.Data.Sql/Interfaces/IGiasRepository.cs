using Dfe.PlanTech.Core.DataTransferObjects;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IGiasRepository
{
    Task<GiasEstablishmentEntity?> GetSingleAcademySchool(int groupUid);
    Task<GroupEstablishmentDTO?> GetGiasGroupByGroupUIDAsync(int groupUid);
}
