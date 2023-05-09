using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contentful.Core;
using Contentful.Core.Search;
using Sts.PlanTech.Infrastructure.Persistence;
using Sts.PlanTech.Infrastructure.Persistence.Querying;

namespace Sts.PlanTech.Infrastructure.Contentful.Persistence;


/// <summary>
/// Encapsulates ContentfulClient functionality, whilst abstracting through the IEntityRepository interface
/// </summary>
/// <see href="IEntityRepository"/>
public class ContentfulRepository : IContentRepository
{
    private readonly IContentfulClient _client;

    public ContentfulRepository(IContentfulClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<IEnumerable<TEntity>> GetEntities<TEntity>(string entityTypeId, IEnumerable<ContentQuery>? queries = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var queryBuilder = QueryBuilders.ByContentType<TEntity>(entityTypeId);

        if (queries != null)
        {
            queryBuilder.WithQueries(queries);
        }

        var entries = await _client.GetEntries<TEntity>(queryBuilder, cancellationToken);

        if (entries == null) return Enumerable.Empty<TEntity>();

        return entries;
    }

    public Task<IEnumerable<TEntity>> GetEntities<TEntity>(IEnumerable<ContentQuery>? queries = null, CancellationToken cancellationToken = default)
        => GetEntities<TEntity>(typeof(TEntity).Name.ToLower(), queries, cancellationToken);

    public async Task<TEntity?> GetEntityById<TEntity>(string id, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

        var entry = await _client.GetEntry<TEntity>(id, null, null, cancellationToken);

        return entry.Result;
    }
}
