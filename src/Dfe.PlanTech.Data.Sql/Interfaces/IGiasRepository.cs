using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IGiasRepository
{
    Task<GiasEstablishmentEntity?> GetSchoolByUrn(int urn);
    Task<GiasEstablishmentEntity?> GetSingleAcademySchool(int groupUid);
    Task<List<GiasEstablishmentEntity>> GetSchoolsForGroupAsync(int groupUid);
}
