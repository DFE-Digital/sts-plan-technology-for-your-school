using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Dfe.PlanTech.Tests.Shared.Fakes;

public sealed class HttpContextBuilder
{
    private readonly ClaimsPrincipalBuilder _principalBuilder = new();
    private readonly Dictionary<string, string> _headers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<object, object?> _items = [];
    private FakeSession _session = new();
    private string _path = "/";

    public HttpContextBuilder WithClaim(string type, string value)
    {
        _principalBuilder.WithClaim(type, value);
        return this;
    }

    public HttpContextBuilder WithClaims(params Claim[] claims)
    {
        _principalBuilder.WithClaims(claims);
        return this;
    }

    public HttpContextBuilder Unauthenticated()
    {
        _principalBuilder.Unauthenticated();
        return this;
    }

    public HttpContextBuilder WithHeader(string name, string value)
    {
        _headers[name] = value;
        return this;
    }

    public HttpContextBuilder WithPath(string path)
    {
        _path = path;
        return this;
    }

    public HttpContextBuilder WithItem(object key, object? value)
    {
        _items[key] = value;
        return this;
    }

    public HttpContextBuilder WithSession(FakeSession session)
    {
        _session = session;
        return this;
    }

    public DefaultHttpContext Build()
    {
        var context = new DefaultHttpContext { User = _principalBuilder.Build() };
        context.Request.Path = _path;

        foreach (var header in _headers)
        {
            context.Request.Headers[header.Key] = header.Value;
        }

        foreach (var item in _items)
        {
            context.Items[item.Key] = item.Value;
        }

        context.Features.Set<ISessionFeature>(new SessionFeature { Session = _session });
        return context;
    }

    private sealed class SessionFeature : ISessionFeature
    {
        public ISession Session { get; set; } = default!;
    }
}
