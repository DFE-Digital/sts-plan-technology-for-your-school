using Contentful.Core;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Helpers;
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
        _client.ContentTypeResolver = new EntityResolver(loggerFactory.CreateLogger<EntityResolver>());
    }

    public async Task<IEnumerable<TEntity>> GetEntities<TEntity>(string entityTypeId, IGetEntitiesOptions? options, CancellationToken cancellationToken = default)
    {
        var queryBuilder = QueryBuilders.BuildQueryBuilder<TEntity>(entityTypeId, options);

        var entries = await _client.GetEntries(queryBuilder, cancellationToken);

        return entries ?? Enumerable.Empty<TEntity>();
    }

    public async Task<IEnumerable<TEntity>> GetEntities<TEntity>(CancellationToken cancellationToken = default)
    => await GetEntities<TEntity>(LowerCaseFirstLetter(typeof(TEntity).Name), null, cancellationToken);

    public async Task<IEnumerable<TEntity>> GetEntities<TEntity>(IGetEntitiesOptions options, CancellationToken cancellationToken = default)
        => await GetEntities<TEntity>(LowerCaseFirstLetter(typeof(TEntity).Name), options, cancellationToken);

    public async Task<TEntity?> GetEntityById<TEntity>(string id, int include = 2, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));

        //There exists a "GetEntry" option for the Contentful client, however the "Include"
        //option doesn't seem to have any effect there - it only seems to return the main parent entry
        //with links to children. This was proving rather useless, so I have used the "GetEntries" option here
        //instead.
        var options = new GetEntitiesOptions(include, new[] {
            new ContentQueryEquals(){
                Field = "sys.id",
                Value = id
        }});

        var entities = (await GetEntities<TEntity>(options, cancellationToken)).ToList();

        if (entities.Count > 1)
        {
            throw new GetEntitiesException($"Found more than 1 entity with id {id}");
        }

        return entities.FirstOrDefault();
    }

    private static string LowerCaseFirstLetter(string toLowerCase) => char.ToLower(toLowerCase[0]) + toLowerCase.Substring(1);
}
