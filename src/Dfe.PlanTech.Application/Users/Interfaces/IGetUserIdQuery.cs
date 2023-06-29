namespace Dfe.PlanTech.Application.Users.Interfaces;

public interface IGetUserIdQuery
{
    Task<int?> GetUserId(string dfeSignInRef);
}
