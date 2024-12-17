using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
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

    public async Task<IEnumerable<TEntity>> GetEntities<TEntity>(CancellationToken cancellationToken = default)
    {
        string contentType = GetContentTypeName<TEntity>();
        var key = $"{contentType}s";

        return await _cache.GetOrCreateAsync(key, async () => await _contentRepository.GetEntities<TEntity>(cancellationToken)) ?? [];
    }

    public async Task<IEnumerable<TEntity>> GetEntities<TEntity>(IGetEntitiesOptions options, CancellationToken cancellationToken = default)
    {
        var contentType = GetContentTypeName<TEntity>();
        var jsonOptions = options.SerializeToRedisKey();
        var key = $"{contentType}{jsonOptions}";

        return await _cache.GetOrCreateAsync(key, async () => await _contentRepository.GetEntities<TEntity>(options, cancellationToken)) ?? [];
    }

    public async Task<TEntity?> GetEntityById<TEntity>(string id, int include = 2, CancellationToken cancellationToken = default)
    {
        return await _contentRepository.GetEntityById<TEntity>(id, include, cancellationToken);
    }

    private static string GetContentTypeName<TEntity>()
    {
        var name = typeof(TEntity).Name;
        return name == "ContentSupportPage" ? name : name.FirstCharToLower();
    }
}
