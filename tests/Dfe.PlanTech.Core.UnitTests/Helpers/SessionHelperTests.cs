using System.Text;
using System.Text.Json;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Helpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Dfe.PlanTech.Core.UnitTests.Helpers;

public class SessionHelperTests
{
    private readonly ISession _session;

    public SessionHelperTests()
    {
        _session = Substitute.For<ISession>();
    }

    [Fact]
    public void SetValue_StoresSerializedValue_WhenKeyAndTypeAreValid_ForInt()
    {
        var value = 42;
        byte[]? storedBytes = null;
        _session
            .When(s => s.Set(SessionConstants.FocusedEstablishmentKey, Arg.Any<byte[]>()))
            .Do(c => storedBytes = c.ArgAt<byte[]>(1));

        _session.SetValue(SessionConstants.FocusedEstablishmentKey, value);

        Assert.NotNull(storedBytes);
        var storedValue = Encoding.UTF8.GetString(storedBytes);
        Assert.Equal(JsonSerializer.Serialize(value), storedValue);
    }

    [Fact]
    public void SetValue_StoresSerializedValue_WhenKeyAndTypeAreValid_ForEnumerable()
    {
        var value = new List<int> { 1, 2, 3 };
        byte[]? storedBytes = null;
        _session
            .When(s => s.Set(SessionConstants.SelectedEstablishmentsKey, Arg.Any<byte[]>()))
            .Do(c => storedBytes = c.ArgAt<byte[]>(1));

        _session.SetValue<IEnumerable<int>>(SessionConstants.SelectedEstablishmentsKey, value);

        Assert.NotNull(storedBytes);
        var storedValue = Encoding.UTF8.GetString(storedBytes);
        Assert.Equal(JsonSerializer.Serialize(value), storedValue);
    }

    [Fact]
    public void SetValue_Throws_WhenKeyIsUnrecognised()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            _session.SetValue("unknown_key", 42)
        );

        Assert.Contains("does not expect that key", ex.Message);
    }

    [Theory]
    [InlineData(SessionConstants.FocusedEstablishmentKey)]
    [InlineData(SessionConstants.SelectedEstablishmentsKey)]
    public void SetValue_Throws_WhenValueTypeIsWrongForKey(string key)
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            _session.SetValue(key, "wrong-type-value")
        );

        Assert.Contains("wrong type", ex.Message);
    }

    [Fact]
    public void GetValue_ReturnsDeserializedValue_WhenKeyExistsAndValueIsPresent_ForInt()
    {
        var expected = 99;
        SetupSessionValue(
            SessionConstants.FocusedEstablishmentKey,
            JsonSerializer.Serialize(expected)
        );

        var result = _session.GetValue(SessionConstants.FocusedEstablishmentKey);

        Assert.NotNull(result);
        Assert.Equal(expected, (int)result);
    }

    [Fact]
    public void GetValue_ReturnsDeserializedValue_WhenKeyExistsAndValueIsPresent_ForEnumerable()
    {
        var expected = new List<int> { 10, 20, 30 };
        SetupSessionValue(
            SessionConstants.SelectedEstablishmentsKey,
            JsonSerializer.Serialize(expected)
        );

        var result = _session.GetValue(SessionConstants.SelectedEstablishmentsKey);

        Assert.NotNull(result);
        Assert.Equal(expected, Assert.IsAssignableFrom<IEnumerable<int>>(result));
    }

    [Fact]
    public void GetValue_ReturnsNull_WhenKeyExistsButNoValueStored()
    {
        SetupSessionValue(SessionConstants.FocusedEstablishmentKey, null);

        var result = _session.GetValue(SessionConstants.FocusedEstablishmentKey);

        Assert.Null(result);
    }

    [Fact]
    public void GetValue_Throws_WhenKeyIsUnrecognised()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => _session.GetValue("unknown_key"));

        Assert.Contains("does not expect that key", ex.Message);
    }

    [Fact]
    public void GetValue_DoesNotCallTryGetValue_WhenKeyIsUnrecognised()
    {
        try
        {
            _session.GetValue("unknown_key");
        }
        catch (InvalidOperationException) { }

        _session.DidNotReceive().TryGetValue(Arg.Any<string>(), out Arg.Any<byte[]?>());
    }

    [Fact]
    public void SetValue_ThenGetValue_RoundTrips_FocusedEstablishment()
    {
        var value = 7;
        byte[]? stored = null;

        _session
            .When(s => s.Set(SessionConstants.FocusedEstablishmentKey, Arg.Any<byte[]>()))
            .Do(c => stored = c.ArgAt<byte[]>(1));

        _session
            .TryGetValue(SessionConstants.FocusedEstablishmentKey, out Arg.Any<byte[]?>())
            .Returns(call =>
            {
                call[1] = stored;
                return stored is not null;
            });

        _session.SetValue(SessionConstants.FocusedEstablishmentKey, value);
        var result = _session.GetValue(SessionConstants.FocusedEstablishmentKey);

        Assert.Equal(value, (int)result!);
    }

    [Fact]
    public void SetValue_ThenGetValue_RoundTrips_SelectedEstablishments()
    {
        var value = new List<int> { 3, 6, 9 };
        byte[]? stored = null;

        _session
            .When(s => s.Set(SessionConstants.SelectedEstablishmentsKey, Arg.Any<byte[]>()))
            .Do(c => stored = c.ArgAt<byte[]>(1));

        _session
            .TryGetValue(SessionConstants.SelectedEstablishmentsKey, out Arg.Any<byte[]?>())
            .Returns(call =>
            {
                call[1] = stored;
                return stored is not null;
            });

        _session.SetValue<IEnumerable<int>>(SessionConstants.SelectedEstablishmentsKey, value);
        var result = _session.GetValue(SessionConstants.SelectedEstablishmentsKey);

        Assert.NotNull(result);
        Assert.Equal(value, Assert.IsAssignableFrom<IEnumerable<int>>(result));
    }

    private void SetupSessionValue(string key, string? serializedValue)
    {
        if (serializedValue is null)
        {
            _session.TryGetValue(key, out Arg.Any<byte[]?>()).Returns(false);
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(serializedValue);
        _session
            .TryGetValue(key, out Arg.Any<byte[]?>())
            .Returns(call =>
            {
                call[1] = bytes;
                return true;
            });
    }
}
