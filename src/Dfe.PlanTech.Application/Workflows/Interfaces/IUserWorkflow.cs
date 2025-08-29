using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Application.Workflows.Interfaces
{
    public interface IUserWorkflow
    {
        Task<SqlUserDto?> GetUserBySignInRefAsync(string dfeSignInRef);
    }
}
