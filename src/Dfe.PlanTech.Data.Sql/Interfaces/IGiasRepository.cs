using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IGiasRepository
{
    Task<GiasEstablishmentEntity?> GetSingleAcademySchool(int groupUid);
    Task<List<GiasEstablishmentEntity>> GetSchoolsForGroupAsync(int groupUid);
    Task<bool> IsSchoolWithinGroupAsync(int groupUid, int schoolUrn);
}
