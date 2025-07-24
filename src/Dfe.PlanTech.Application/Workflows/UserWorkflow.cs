using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Repositories;

namespace Dfe.PlanTech.Application.Workflows;

public class UserWorkflow
{
    private readonly UserRepository _userRepository;

    public UserWorkflow(
        UserRepository userRepository
    )
    {
        _userRepository = userRepository;
    }

    public async Task<SqlUserDto?> GetUserBySignInRefAsync(string dfeSignInRef)
    {
        var user = await _userRepository.GetUserBySignInRefAsync(dfeSignInRef);
        return user?.AsDto();
    }
}

