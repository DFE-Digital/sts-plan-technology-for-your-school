using System.Security.Claims;

namespace Dfe.PlanTech.Tests.Shared.Fakes;

public sealed class ClaimsPrincipalBuilder
{
    private readonly List<Claim> _claims = [];
    private bool _isAuthenticated = true;

    public ClaimsPrincipalBuilder WithClaim(string type, string value)
    {
        _claims.Add(new Claim(type, value));
        return this;
    }

    public ClaimsPrincipalBuilder WithClaims(params Claim[] claims)
    {
        _claims.AddRange(claims);
        return this;
    }

    public ClaimsPrincipalBuilder Unauthenticated()
    {
        _isAuthenticated = false;
        return this;
    }

    public ClaimsPrincipal Build()
    {
        var identity = new ClaimsIdentity(_claims, _isAuthenticated ? "Test" : null);
        return new ClaimsPrincipal(identity);
    }
}
