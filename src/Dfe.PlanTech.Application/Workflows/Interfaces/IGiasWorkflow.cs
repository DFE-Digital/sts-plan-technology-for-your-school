using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Application.Workflows.Interfaces;

public interface IGiasWorkflow
{
    Task<SqlGiasEstablishmentDto?> GetSchoolByUrn(string urn);
    Task<IEnumerable<SqlGiasEstablishmentDto>> GetSchoolsInGroup(int groupUid);
    Task<SqlGiasEstablishmentDto?> GetSingleAcademySchool(int groupUid);
    Task<bool> HasAllSchoolsInGroup(int groupUid, IEnumerable<int> schoolUrns);
}
