using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Providers;

public class MicrocopyProvider(
    ILogger<MicrocopyProvider> logger,
    IContentfulService contentfulService
    ) : IMicrocopyProvider
{
    private readonly ILogger _logger = logger
        ?? throw new ArgumentNullException(nameof(logger));
    private readonly IContentfulService _contentfulService = contentfulService
        ?? throw new ArgumentNullException(nameof(contentfulService));

    private Task<Dictionary<string, MicrocopyModel>>? _entriesTask = null;

    public async Task<Dictionary<string, MicrocopyModel>> CreateRecordsAsync()
    {
        var entries = await _contentfulService.GetMicrocopyEntriesAsync();

        return entries.ToDictionary(entry => entry.Key, CreateMicrocopyRecord);
    }

    public async Task<MicrocopyModel?> GetRecordByKeyAsync(string key)
    {
        var records = await CreateRecordsInternalAsync();

        if (!records.TryGetValue(key, out MicrocopyModel? record))
        {
            _logger.LogWarning("Microcopy record with key '{Key}' was not found", key);
        }

        return record;
    }

    private MicrocopyModel CreateMicrocopyRecord(MicrocopyEntry entry)
    {
        ContentfulMicrocopyConstants.FallbackText.TryGetValue(entry.Key, out var fallbackText);
        ContentfulMicrocopyConstants.Variables.TryGetValue(entry.Key, out var variables);

        if (fallbackText is null)
        {
            _logger.LogWarning("Cannot find fallback text for microcopy with key '{Key}'", entry.Key);
        }

        return new MicrocopyModel(
            entry.Key,
            entry.Value,
            fallbackText is null
                ? string.Empty
                : ContentfulMicrocopyConstants.FallbackText[entry.Key],
            variables is null ? [] : [.. variables]
        );
    }


    // Refactor this for readability
    public async Task<string> GetTextByKeyAsync(
        string key,
        Dictionary<string, string>? dynamicValues = null
        )
    {
        var record = await GetRecordByKeyAsync(key);
        string? noRecordFallbackText = null;

        if (record is null
            && (!ContentfulMicrocopyConstants.FallbackText.TryGetValue(
                key,
                out noRecordFallbackText) || noRecordFallbackText is null
            )
        )
        {
            throw new KeyNotFoundException($"Microcopy record with key '{key}' was not found.");
        }

        return record?.GetText(dynamicValues) ?? noRecordFallbackText!;
    }

    private Task<Dictionary<string, MicrocopyModel>> CreateRecordsInternalAsync()
    {
        _entriesTask ??= CreateRecordsAsync();
        return _entriesTask;
    }
}
