using Dfe.PlanTech.Application.Providers;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.UnitTests.Shared.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Providers;

public class MicrocopyProviderTests
{
    private readonly IContentfulService _contentful = Substitute.For<IContentfulService>();
    private readonly ILogger<MicrocopyProvider> _logger = Substitute.For<ILogger<MicrocopyProvider>>();

    private MicrocopyProvider CreateServiceUnderTest() => new(_logger, _contentful);

    [Fact]
    public async Task CreateRecordsAsync_ConvertsEntriesToRecords()
    {
        // Arrange
        var entry = new MicrocopyEntry { Key = "Key 1", Value = "Value 1" };
        ContentfulMicrocopyConstants.FallbackText["Key 1"] = "Fallback 1";
        _contentful.GetMicrocopyEntriesAsync().Returns(new List<MicrocopyEntry> { entry });

        var sut = CreateServiceUnderTest();

        // Act
        var result = await sut.CreateRecordsAsync();

        // Assert
        Assert.Single(result);
        var record = result["Key 1"];

        Assert.Equal("Key 1", record.Key);
        Assert.Equal("Value 1", record.Value);
        Assert.Equal("Fallback 1", record.FallbackText);
    }

    [Fact]
    public async Task GetRecordByKeyAsync_WhenMissing_LogsWarning()
    {
        // Arrange
        var noMicrocopyEntries = new List<MicrocopyEntry>();
        _contentful.GetMicrocopyEntriesAsync()
            .Returns(noMicrocopyEntries);

        var sut = CreateServiceUnderTest();

        // Act
        var result = await sut.GetRecordByKeyAsync("Missing key 1");

        // Assert
        Assert.Null(result);

        _logger.Received(1);
        var logMessage = _logger.ReceivedLogMessages().FirstOrDefault();
        Assert.NotNull(logMessage);
        Assert.Equal("Microcopy record with key 'Missing key 1' was not found", logMessage.Message);
    }

    [Fact]
    public async Task GetRecordByKeyAsync_WhenNoFallback_LogsWarning()
    {
        // Arrange
        var entry = new MicrocopyEntry { Key = "Key 2", Value = "Value 2" };
        ContentfulMicrocopyConstants.FallbackText["Key 2"] = null!;
        _contentful.GetMicrocopyEntriesAsync()
            .Returns(new List<MicrocopyEntry>{ entry});

        var sut = CreateServiceUnderTest();

        // Act
        var result = await sut.GetRecordByKeyAsync("Key 2");

        // Assert
        Assert.Equal(result?.Key, entry.Key);

        _logger.Received(1);
        var logMessage = _logger.ReceivedLogMessages().FirstOrDefault();
        Assert.NotNull(logMessage);
        Assert.Equal("Cannot find fallback text for microcopy with key 'Key 2'", logMessage.Message);
    }

    [Fact]
    public async Task GetTextByKeyAsync_WhenRecordMissing_UsesFallbackText()
    {
        // Arrange
        ContentfulMicrocopyConstants.FallbackText["Missing key 2"] = "Fallback text";
        var noMicrocopyEntries = new List<MicrocopyEntry>();
        _contentful.GetMicrocopyEntriesAsync()
            .Returns(noMicrocopyEntries);

        var sut = CreateServiceUnderTest();

        // Act
        var result = await sut.GetTextByKeyAsync("Missing key 2");

        // Assert
        Assert.Equal("Fallback text", result);
    }

    [Fact]
    public async Task GetTextByKeyAsync_NoRecordNoFallback_Throws()
    {
        // Arrange
        var noMicrocopyEntries = new List<MicrocopyEntry>();
        _contentful.GetMicrocopyEntriesAsync()
            .Returns(noMicrocopyEntries);

        var sut = CreateServiceUnderTest();

        // Act + Assert
        var result = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await sut.GetTextByKeyAsync("Missing completely"));
        Assert.Equal("Microcopy record with key 'Missing completely' was not found.", result.Message);
    }

    [Fact]
    public async Task GetRecordByKeyAsync_CachesEntries()
    {
        // Arrange
        var entry = new MicrocopyEntry { Key = "Key 4", Value = "Value 4" };
        _contentful.GetMicrocopyEntriesAsync()
            .Returns(new List<MicrocopyEntry> { entry });

        var sut = CreateServiceUnderTest();

        // Act
        await sut.GetRecordByKeyAsync("Key 4");
        await sut.GetRecordByKeyAsync("Key 4");
        await sut.GetTextByKeyAsync("Key 4");

        // Assert
        await _contentful.Received(1).GetMicrocopyEntriesAsync();
    }
}
