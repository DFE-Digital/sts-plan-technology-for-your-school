using Dfe.PlanTech.Core.Caching.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Content.Options;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Data.Contentful.Helpers;
using Dfe.PlanTech.Data.Contentful.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Data.Contentful.Persistence;

/// <summary>
/// Encapsulates ContentfulClient functionality, whilst abstracting through the IEntryRepository interface
/// </summary>
public class CachedContentfulRepository(
    [FromKeyedServices(KeyedServiceConstants.ContentfulRepository)] IContentfulRepository contentfulRepository,
    ICmsCache cmsCache

) : IContentfulRepository
{
    private readonly IContentfulRepository _contentfulRepository = contentfulRepository ?? throw new ArgumentNullException(nameof(contentfulRepository));
    private readonly ICmsCache _cmsCache = cmsCache ?? throw new ArgumentNullException(nameof(cmsCache));

    public async Task<int> GetEntriesCount<TEntry>()
    {
        return await _contentfulRepository.GetEntriesCount<TEntry>();
    }
    public async Task<IEnumerable<TEntry>> GetEntriesAsync<TEntry>()
    {
        string contentType = ContentTypeHelper.GetContentTypeName<TEntry>();
        var key = $"{contentType}s";

        return await _cmsCache.GetOrCreateAsync(key, async () => await _contentfulRepository.GetEntriesAsync<TEntry>()) ?? [];
    }

    public async Task<IEnumerable<TEntry>> GetEntriesAsync<TEntry>(GetEntriesOptions options)
    {
        var contentType = ContentTypeHelper.GetContentTypeName<TEntry>();
        var jsonOptions = options.SerializeToRedisFormat();
        var key = $"{contentType}{jsonOptions}";

        return await _cmsCache.GetOrCreateAsync(key, async () => await _contentfulRepository.GetEntriesAsync<TEntry>(options)) ?? [];
    }
    public async Task<IEnumerable<TEntry>> GetPaginatedEntriesAsync<TEntry>(GetEntriesOptions options)
    {
        return await _contentfulRepository.GetPaginatedEntriesAsync<TEntry>(options) ?? [];
    }

    public async Task<TEntry?> GetEntryByIdAsync<TEntry>(string id, int include = 2)
    {
        var options = GetEntryByIdOptions(id, include);
        var entries = (await GetEntriesAsync<TEntry>(options)).ToList();

        if (entries.Count > 1)
            throw new GetEntriesException($"Found more than 1 entity with id {id}");

        return entries.FirstOrDefault();
    }

    public GetEntriesOptions GetEntryByIdOptions(string id, int include = 2)
    {
        return _contentfulRepository.GetEntryByIdOptions(id, include);
    }
}
