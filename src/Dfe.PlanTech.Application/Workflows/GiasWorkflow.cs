using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class GiasWorkflow(IGiasRepository giasRepository) : IGiasWorkflow
{
    private readonly IGiasRepository _giasRepository =
        giasRepository ?? throw new ArgumentNullException(nameof(giasRepository));

    public async Task<SqlGiasEstablishmentDto?> GetSingleAcademySchool(int groupUid)
    {
        var school = await _giasRepository.GetSingleAcademySchool(groupUid);
        return school?.AsDto();
    }
}
