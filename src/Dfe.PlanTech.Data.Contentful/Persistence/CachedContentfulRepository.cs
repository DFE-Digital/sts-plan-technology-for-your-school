using Dfe.PlanTech.Application.Workflows.Options;
using Dfe.PlanTech.Core.Caching.Interfaces;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Data.Contentful.Helpers;
using Dfe.PlanTech.Data.Contentful.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Repositories;

/// <summary>
/// Encapsulates ContentfulClient functionality, whilst abstracting through the IEntryRepository interface
/// </summary>
/// <see href="IEntryRepository"/>
public class CachedContentfulRepository : IContentRepository
{
    private readonly IContentRepository _contentRepository;
    private readonly ICmsCache _cache;

    public CachedContentfulRepository([FromKeyedServices("contentfulRepository")] IContentRepository contentRepository, ICmsCache cache)
    {
        _contentRepository = contentRepository ?? throw new ArgumentNullException(nameof(contentRepository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<int> GetEntriesCount<TEntry>()
    {
        return await _contentRepository.GetEntriesCount<TEntry>();
    }
    public async Task<IEnumerable<TEntry>> GetEntriesAsync<TEntry>()
    {
        string contentType = GetContentTypeName<TEntry>();
        var key = $"{contentType}s";

        return await _cache.GetOrCreateAsync(key, async () => await _contentRepository.GetEntriesAsync<TEntry>()) ?? [];
    }

    public async Task<IEnumerable<TEntry>> GetEntries<TEntry>(GetEntriesOptions options)
    {
        var contentType = GetContentTypeName<TEntry>();
        var jsonOptions = options.SerializeToRedisFormat();
        var key = $"{contentType}{jsonOptions}";

        return await _cache.GetOrCreateAsync(key, async () => await _contentRepository.GetEntries<TEntry>(options)) ?? [];
    }
    public async Task<IEnumerable<TEntry>> GetPaginatedEntries<TEntry>(GetEntriesOptions options)
    {
        return await _contentRepository.GetPaginatedEntries<TEntry>(options) ?? [];
    }

    public async Task<TEntry?> GetEntryById<TEntry>(string id, int include = 2)
    {
        var options = GetEntryByIdOptions(id, include);
        var entries = (await GetEntries<TEntry>(options)).ToList();

        if (entries.Count > 1)
            throw new GetEntriesException($"Found more than 1 entity with id {id}");

        return entries.FirstOrDefault();
    }

    public GetEntriesOptions GetEntryByIdOptions(string id, int include = 2)
    {
        return _contentRepository.GetEntryByIdOptions(id, include);
    }

    private static string GetContentTypeName<TEntry>()
    {
        var name = typeof(TEntry).Name;
        return name == "ContentSupportPage" ? name : name.FirstCharToLower();
    }
}
