using Dfe.PlanTech.Domain.Users.Models;

namespace Dfe.PlanTech.Domain.Users.Interfaces;

public interface ICreateUserCommand
{
    Task<int> CreateUser(RecordUserSignInDto createUserDTO);
}
