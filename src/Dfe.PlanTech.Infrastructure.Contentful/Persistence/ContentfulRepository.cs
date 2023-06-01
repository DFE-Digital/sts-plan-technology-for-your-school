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

    public async Task<IEnumerable<TEntity>> GetEntities<TEntity>(string entityTypeId, IEnumerable<IContentQuery>? queries = null, CancellationToken cancellationToken = default)
    {
        var queryBuilder = QueryBuilders.ByContentType<TEntity>(entityTypeId);

        if (queries != null)
        {
            queryBuilder.WithQueries(queries);
        }

        queryBuilder.Include(4);
        var entries = await _client.GetEntries(queryBuilder, cancellationToken);

        return entries ?? Enumerable.Empty<TEntity>();
    }

    public Task<IEnumerable<TEntity>> GetEntities<TEntity>(IEnumerable<IContentQuery>? queries = null, CancellationToken cancellationToken = default)
        => GetEntities<TEntity>(typeof(TEntity).Name.ToLower(), queries, cancellationToken);

    public async Task<TEntity?> GetEntityById<TEntity>(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

        var entry = await _client.GetEntry<TEntity>(id, null, null, cancellationToken);

        return entry.Result;
    }
}
