using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Infrastructure.Data.Sql.Repositories;

namespace Dfe.PlanTech.Application.Workflows;

public class UserWorkflow
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly EstablishmentRepository _establishmentRepository;
    private readonly UserRepository _userRepository;

    public UserWorkflow(
        IHttpContextAccessor httpContextAccessor,
        EstablishmentRepository establishmentRepository,
        UserRepository userRepository
    )
    {
        _contextAccessor = httpContextAccessor;
        _establishmentRepository = establishmentRepository;
        _userRepository = userRepository;
    }

    public async Task<SqlUserDto?> GetUserBySignInRefAsync(string dfeSignInRef)
    {
        var user = await _userRepository.GetUserBySignInRefAsync(dfeSignInRef);
        return user?.AsDto();
    }
}

