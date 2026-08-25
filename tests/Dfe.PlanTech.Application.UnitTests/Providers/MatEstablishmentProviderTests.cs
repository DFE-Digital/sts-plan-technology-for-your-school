using System.Text.Json;
using Dfe.PlanTech.Application.Providers;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Providers;

public class MatEstablishmentProviderTests
{
    private readonly IEstablishmentService _establishmentService =
        Substitute.For<IEstablishmentService>();

    [Fact]
    public void IsBulkAssessment_WhenSessionContainsSelectedEstablishments_ReturnsTrue()
    {
        var session = new TestSession();

        session.Set(
            SessionConstants.SelectedEstablishmentsKey,
            JsonSerializer.SerializeToUtf8Bytes(
                new[]
                {
                    101,
                    102,
                }
            )
        );

        var sut = CreateServiceUnderTest(session);

        var result = sut.IsBulkAssessment();

        Assert.True(result);
    }

    [Fact]
    public void IsBulkAssessment_WhenSessionContainsOneSelectedEstablishment_ReturnsTrue()
    {
        var session = new TestSession();

        session.Set(
            SessionConstants.SelectedEstablishmentsKey,
            JsonSerializer.SerializeToUtf8Bytes(
                new[]
                {
                    101
                }
            )
        );

        var sut = CreateServiceUnderTest(session);

        var result = sut.IsBulkAssessment();

        Assert.True(result);
    }

    [Fact]
    public void IsBulkAssessment_WhenSessionContainsNoSelectedEstablishments_ReturnsFalse()
    {
        var session = new TestSession();

        session.Set(
            SessionConstants.SelectedEstablishmentsKey,
            JsonSerializer.SerializeToUtf8Bytes(Array.Empty<int>())
        );

        var sut = CreateServiceUnderTest(session);

        var result = sut.IsBulkAssessment();

        Assert.False(result);
    }

    [Fact]
    public void IsBulkAssessment_WhenSessionValueDoesNotExist_ReturnsFalse()
    {
        var session = new TestSession();

        var sut = CreateServiceUnderTest(session);

        var result = sut.IsBulkAssessment();

        Assert.False(result);
    }

    private MatEstablishmentProvider CreateServiceUnderTest(
        ISession session
    )
    {
        var httpContext = new DefaultHttpContext
        {
            Session = session,
        };

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = httpContext,
        };

        return new MatEstablishmentProvider(
            httpContextAccessor,
            _establishmentService
        );
    }

    private sealed class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _values = [];

        public bool IsAvailable => true;

        public string Id { get; } = Guid.NewGuid().ToString();

        public IEnumerable<string> Keys => _values.Keys;

        public void Clear()
        {
            _values.Clear();
        }

        public Task CommitAsync(
            CancellationToken cancellationToken = default
        )
        {
            return Task.CompletedTask;
        }

        public Task LoadAsync(
            CancellationToken cancellationToken = default
        )
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _values.Remove(key);
        }

        public void Set(
            string key,
            byte[] value
        )
        {
            _values[key] = value;
        }

        public bool TryGetValue(
            string key,
            out byte[] value
        )
        {
            return _values.TryGetValue(key, out value!);
        }
    }
}
