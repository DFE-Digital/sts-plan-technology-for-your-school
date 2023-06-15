namespace Dfe.PlanTech.Domain.SignIn.Models;

public interface IDfeSignInConfiguration
{
    string Authority { get; set; }
    string MetaDataUrl { get; set; }
    string APIServiceProxyUrl { get; set; }
    string CallbackUrl { get; set; }
    string ClientId { get; set; }
    string ClientSecret { get; set; }
    string CookieName { get; set; }
    int CookieExpireTimeSpanInMinutes { get; set; }
    bool SlidingExpiration { get; set; }
    string AccessDeniedPath { get; set; }
    bool GetClaimsFromUserInfoEndpoint { get; set; }
    bool SaveTokens { get; set; }
    IList<string> Scopes { get; set; }
    string SignoutCallbackUrl { get; set; }
    string SignoutRedirectUrl { get; set; }
    bool DiscoverRolesWithPublicApi { get; set; }
}
