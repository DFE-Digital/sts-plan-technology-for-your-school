using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Application.Workflows.Interfaces;

public interface IGiasWorkflow
{
    Task<SqlGiasEstablishmentDto?> GetSingleAcademySchool(int groupUid);
}
