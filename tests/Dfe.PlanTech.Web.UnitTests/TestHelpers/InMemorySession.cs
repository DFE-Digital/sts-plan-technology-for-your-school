using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Dfe.PlanTech.Web.UnitTests.TestHelpers;

/// <summary>
/// Minimal in-memory ISession for unit tests. Because it implements the real
/// Set/TryGetValue contract, any session extension methods (SetString, GetString,
/// or our own JSON SetValue/GetValue helpers) work against it unchanged.
/// </summary>
public sealed class InMemorySession : ISession
{
    private readonly Dictionary<string, byte[]> _store = new(StringComparer.Ordinal);

    public bool IsAvailable => true;
    public string Id { get; } = Guid.NewGuid().ToString();
    public IEnumerable<string> Keys => _store.Keys;

    public void Clear() => _store.Clear();

    public void Remove(string key) => _store.Remove(key);

    public void Set(string key, byte[] value) => _store[key] = value;

    public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value) =>
        _store.TryGetValue(key, out value);

    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public static class HttpContextSessionExtensions
{
    /// <summary>
    /// Attaches an in-memory session to the context so ctx.Session works,
    /// and returns it so a test can seed or inspect values.
    /// </summary>
    public static InMemorySession AttachSession(this HttpContext context)
    {
        var session = new InMemorySession();
        context.Features.Set<ISessionFeature>(new TestSessionFeature { Session = session });
        return session;
    }

    private sealed class TestSessionFeature : ISessionFeature
    {
        public ISession Session { get; set; } = default!;
    }
}
