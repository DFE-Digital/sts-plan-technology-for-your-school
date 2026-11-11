using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class GiasWorkflow(IGiasRepository giasRepository) : IGiasWorkflow
{
    private readonly IGiasRepository _giasRepository =
        giasRepository ?? throw new ArgumentNullException(nameof(giasRepository));

    public async Task<IEnumerable<SqlGiasEstablishmentDto>> GetSchoolsInGroup(int groupUid)
    {
        var groupSchools = await _giasRepository.GetSchoolsInGroup(groupUid);
        return groupSchools.Select(gs => gs.AsDto());
    }

    public async Task<SqlGiasEstablishmentDto?> GetSingleAcademySchool(int groupUid)
    {
        var school = await _giasRepository.GetSingleAcademySchool(groupUid);
        return school?.AsDto();
    }

    public async Task<bool> HasAllSchoolsInGroup(int groupUid, IEnumerable<int> schoolUrns)
    {
        var groupSchools = await _giasRepository.GetSchoolsInGroup(groupUid);

        return groupSchools.Select(gs => gs.Urn).ToHashSet().IsSupersetOf(schoolUrns);
    }
}
