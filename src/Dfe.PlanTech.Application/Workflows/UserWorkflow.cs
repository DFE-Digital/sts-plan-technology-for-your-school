using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class UserWorkflow(
    IUserRepository userRepository
) : IUserWorkflow
{
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

    public async Task<SqlUserDto?> GetUserBySignInRefAsync(string dfeSignInRef)
    {
        var user = await _userRepository.GetUserBySignInRefAsync(dfeSignInRef);
        return user?.AsDto();
    }
}

