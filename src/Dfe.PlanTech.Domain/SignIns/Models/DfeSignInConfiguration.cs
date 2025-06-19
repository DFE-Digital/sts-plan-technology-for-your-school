namespace Dfe.PlanTech.Domain.SignIns.Models;

public sealed class DfeSignInConfiguration
{
    /// <inheritdoc/>
    public string Authority { get; set; } = null!;

    /// <inheritdoc/>
    public string MetaDataUrl { get; set; } = null!;

    /// <inheritdoc/>
    public string CallbackUrl { get; set; } = null!;

    /// <inheritdoc/>
    public string ClientId { get; set; } = null!;

    /// <inheritdoc/>
    public string ClientSecret { get; set; } = null!;

    /// <inheritdoc/>
    public string CookieName { get; set; } = null!;

    /// <inheritdoc/>
    public int CookieExpireTimeSpanInMinutes { get; set; }

    /// <inheritdoc/>
    public bool SlidingExpiration { get; set; }

    /// <inheritdoc/>
    public string AccessDeniedPath { get; set; } = null!;

    /// <inheritdoc/>
    public bool GetClaimsFromUserInfoEndpoint { get; set; }

    /// <inheritdoc/>
    public bool SaveTokens { get; set; }

    /// <inheritdoc/>
    public List<string> Scopes { get; set; } = null!;

    /// <inheritdoc/>
    public string SignoutCallbackUrl { get; set; } = null!;

    /// <inheritdoc/>
    public string SignoutRedirectUrl { get; set; } = null!;

    public string FrontDoorUrl { get; init; } = null!;

    public bool SkipUnrecognizedRequests { get; set; } = true;
}
