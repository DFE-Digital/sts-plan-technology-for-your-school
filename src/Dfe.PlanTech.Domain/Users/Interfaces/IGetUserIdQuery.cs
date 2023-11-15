namespace Dfe.PlanTech.Domain.Users.Interfaces;

public interface IGetUserIdQuery
{
    Task<int?> GetUserId(string dfeSignInRef);
}
