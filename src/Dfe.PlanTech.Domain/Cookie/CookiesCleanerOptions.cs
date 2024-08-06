namespace Dfe.PlanTech.Domain.Cookie;

public record CookiesCleanerOptions
{
    public required string[] EssentialCookies { get; init; }
}
