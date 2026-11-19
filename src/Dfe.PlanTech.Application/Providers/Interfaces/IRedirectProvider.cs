namespace Dfe.PlanTech.Application.Providers.Interfaces
{
    public interface IRedirectProvider
    {
        bool IsStaticPath(string path);
        Task<string?> TryGetRedirect(string redirectFrom);
    }
}
