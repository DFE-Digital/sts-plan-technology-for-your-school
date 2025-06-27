using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Infrastructure.Contentful.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Infrastructure.Contentful.Persistence;

/// <summary>
/// Encapsulates ContentfulClient functionality, whilst abstracting through the IEntityRepository interface
/// </summary>
/// <see href="IEntityRepository"/>
public class CachedContentfulRepository : IContentRepository
{
    private readonly IContentRepository _contentRepository;
    private readonly ICmsCache _cache;

    public CachedContentfulRepository([FromKeyedServices("contentfulRepository")] IContentRepository contentRepository, ICmsCache cache)
    {
        _contentRepository = contentRepository ?? throw new ArgumentNullException(nameof(contentRepository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<int> GetEntitiesCount<TEntity>()
    {
        return await _contentRepository.GetEntitiesCount<TEntity>();
    }
    public async Task<IEnumerable<TEntity>> GetEntities<TEntity>()
    {
        string contentType = GetContentTypeName<TEntity>();
        var key = $"{contentType}s";

        return await _cache.GetOrCreateAsync(key, async () => await _contentRepository.GetEntities<TEntity>()) ?? [];
    }

    public async Task<IEnumerable<TEntity>> GetEntities<TEntity>(GetEntriesOptions options)
    {
        var contentType = GetContentTypeName<TEntity>();
        var jsonOptions = options.SerializeToRedisFormat();
        var key = $"{contentType}{jsonOptions}";

        return await _cache.GetOrCreateAsync(key, async () => await _contentRepository.GetEntities<TEntity>(options, )) ?? [];
    }
    public async Task<IEnumerable<TEntity>> GetPaginatedEntities<TEntity>(GetEntriesOptions options)
    {
        return await _contentRepository.GetPaginatedEntities<TEntity>(options, ) ?? [];
    }

    public async Task<TEntity?> GetEntityById<TEntity>(string id, int include = 2)
    {
        var options = GetEntityByIdOptions(id, include);
        var entities = (await GetEntities<TEntity>(options, )).ToList();

        if (entities.Count > 1)
            throw new GetEntriesException($"Found more than 1 entity with id {id}");

        return entities.FirstOrDefault();
    }

    public GetEntriesOptions GetEntityByIdOptions(string id, int include = 2)
    {
        return _contentRepository.GetEntityByIdOptions(id, include);
    }

    private static string GetContentTypeName<TEntity>()
    {
        var name = typeof(TEntity).Name;
        return name == "ContentSupportPage" ? name : name.FirstCharToLower();
    }
}
