using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IGiasRepository
{
    Task<List<GiasEstablishmentEntity>> GetSchoolsInGroup(int groupUid);

    Task<GiasEstablishmentEntity?> GetSingleAcademySchool(int groupUid);
}
