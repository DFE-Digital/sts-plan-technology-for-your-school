namespace Dfe.PlanTech.Application.Configuration;

public sealed class DfeSignInConfiguration
{
    public string Authority { get; set; } = null!;

    public string MetaDataUrl { get; set; } = null!;

    public string CallbackUrl { get; set; } = null!;

    public string ClientId { get; set; } = null!;

    public string ClientSecret { get; set; } = null!;

    public string CookieName { get; set; } = null!;

    public int CookieExpireTimeSpanInMinutes { get; set; }

    public bool SlidingExpiration { get; set; }

    public string AccessDeniedPath { get; set; } = null!;

    public bool GetClaimsFromUserInfoEndpoint { get; set; }

    public bool SaveTokens { get; set; }

    public IList<string> Scopes { get; set; } = null!;

    public string SignoutCallbackUrl { get; set; } = null!;

    public string SignoutRedirectUrl { get; set; } = null!;

    public string FrontDoorUrl { get; init; } = null!;

    public bool SkipUnrecognizedRequests { get; set; } = true;
}
