using Dfe.PlanTech.Domain.Users.Models;

namespace Dfe.PlanTech.Application.Persistence.Interfaces;

public interface ICreateUserCommand
{
    Task<int> CreateUser(RecordUserSignInDto createUserDTO);
}
