namespace Dfe.PlanTech.Application.Users.Interfaces
{
    public interface IUser
    {
        Task<int?> GetCurrentUserId();

        Task<int> GetEstablishmentId();
    }
}
