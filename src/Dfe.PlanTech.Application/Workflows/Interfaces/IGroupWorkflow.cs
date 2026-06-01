using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Application.Workflows.Interfaces;

public interface IGroupWorkflow
{
    Task<List<SqlSubmissionDto>> GetGroupCompletedSubmissions(int[] establishmentIds);

}
