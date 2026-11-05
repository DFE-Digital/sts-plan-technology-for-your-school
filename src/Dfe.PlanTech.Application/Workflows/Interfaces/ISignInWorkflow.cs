using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Application.Workflows.Interfaces;

public interface ISignInWorkflow
{
    Task<SqlSignInDto> CreateSignInAsync(int userId, int? establishmentId = null);
}
