namespace Dfe.PlanTech.Domain.Users.Interfaces;

public interface ICreateUserCommand
{
    Task<int> CreateUser(string dfeSignInRef);
}
