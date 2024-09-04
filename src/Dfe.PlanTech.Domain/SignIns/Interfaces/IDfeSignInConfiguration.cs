namespace Dfe.PlanTech.Domain.SignIns.Models;

public interface IDfeSignInConfiguration
{
    public string Authority { get; set; }
    public string MetaDataUrl { get; set; }
    public string APIServiceProxyUrl { get; set; }
    public string CallbackUrl { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string CookieName { get; set; }
    public int CookieExpireTimeSpanInMinutes { get; set; }
    public bool SlidingExpiration { get; set; }
    public string AccessDeniedPath { get; set; }
    public bool GetClaimsFromUserInfoEndpoint { get; set; }
    public bool SaveTokens { get; set; }
    public IList<string> Scopes { get; set; }
    public string SignoutCallbackUrl { get; set; }
    public string SignoutRedirectUrl { get; set; }
    public string FrontDoorUrl { get; init; }
    public string ApiSecret { get; set; }
}
