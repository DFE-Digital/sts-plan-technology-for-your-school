using Contentful.Core;
using Contentful.Core.Configuration;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Contentful.Persistence;

/// <summary>
/// Encapsulates ContentfulClient functionality, whilst abstracting through the IEntityRepository interface
/// </summary>
/// <see href="IEntityRepository"/>
public class ContentfulRepository : IContentRepository
{
    private readonly IContentfulClient _client;

    public ContentfulRepository(ILoggerFactory loggerFactory, IContentfulClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _client.ContentTypeResolver = new EntityResolver(loggerFactory.CreateLogger<IContentTypeResolver>());
    }

    public async Task<IEnumerable<TEntity>> GetEntities<TEntity>(string entityTypeId, IGetEntitiesOptions? options, CancellationToken cancellationToken = default)
    {
        var queryBuilder = QueryBuilders.BuildQueryBuilder<TEntity>(entityTypeId, options);

        var entries = await _client.GetEntries(queryBuilder, cancellationToken);

        return entries ?? Enumerable.Empty<TEntity>();
    }

    public Task<IEnumerable<TEntity>> GetEntities<TEntity>(CancellationToken cancellationToken = default)
    => GetEntities<TEntity>(typeof(TEntity).Name.ToLower(), null, cancellationToken);

    public Task<IEnumerable<TEntity>> GetEntities<TEntity>(IGetEntitiesOptions options, CancellationToken cancellationToken = default)
        => GetEntities<TEntity>(typeof(TEntity).Name.ToLower(), options, cancellationToken);

    public async Task<TEntity?> GetEntityById<TEntity>(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

        var entry = await _client.GetEntry<TEntity>(id, null, null, cancellationToken);

        return entry.Result;
    }
}
