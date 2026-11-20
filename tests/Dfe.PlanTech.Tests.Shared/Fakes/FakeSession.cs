using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Dfe.PlanTech.Tests.Shared.Fakes;

public sealed class FakeSession : ISession
{
    private readonly Dictionary<string, byte[]> _store = new(StringComparer.Ordinal);

    public bool IsAvailable => true;
    public string Id => "fake-session";
    public IEnumerable<string> Keys => _store.Keys;

    public void Clear() => _store.Clear();

    public void Remove(string key) => _store.Remove(key);

    public void Set(string key, byte[] value) => _store[key] = value;

    public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value) =>
        _store.TryGetValue(key, out value);

    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public static class FakeSessionExtensions
{
    public static FakeSession AttachSession(this HttpContext context)
    {
        var session = new FakeSession();
        context.Features.Set<ISessionFeature>(new TestSessionFeature { Session = session });
        return session;
    }

    private sealed class TestSessionFeature : ISessionFeature
    {
        public ISession Session { get; set; } = default!;
    }
}
