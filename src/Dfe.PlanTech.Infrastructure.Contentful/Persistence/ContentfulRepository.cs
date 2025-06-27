using Contentful.Core;
using Contentful.Core.Models;
using Contentful.Core.Search;
using Dfe.PlanTech.Application.Options;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Infrastructure.Contentful.Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Infrastructure.Contentful.Persistence;

/// <summary>
/// Encapsulates ContentfulClient functionality, whilst abstracting through the IEntityRepository interface
/// </summary>
/// <see href="IEntityRepository"/>
public class ContentfulRepository : IContentRepository
{
    private readonly IContentfulClient _client;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly AutomatedTestingOptions _automatedTestingOptions;
    private readonly ILogger<ContentfulRepository> _logger;

    public ContentfulRepository(ILoggerFactory loggerFactory, IContentfulClient client, IHostEnvironment hostEnvironment, IOptions<AutomatedTestingOptions> automatedTestingOptions)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _hostEnvironment = hostEnvironment;
        _automatedTestingOptions = automatedTestingOptions.Value;
        _client.ContentTypeResolver = new EntryResolver(loggerFactory.CreateLogger<EntryResolver>());
        _logger = loggerFactory.CreateLogger<ContentfulRepository>();
    }

    public async Task<IEnumerable<TEntity>> GetEntities<TEntity>(CancellationToken cancellationToken = default)
        => await GetEntities<TEntity>(GetContentTypeName<TEntity>(), null, cancellationToken);

    public async Task<IEnumerable<TEntity>> GetEntities<TEntity>(IGetEntriesOptions options, CancellationToken cancellationToken = default)
        => await GetEntities<TEntity>(GetContentTypeName<TEntity>(), options, cancellationToken);

    public async Task<IEnumerable<TEntity>> GetEntities<TEntity>(string entityTypeId, IGetEntriesOptions? options, CancellationToken cancellationToken = default)
    {
        var queryBuilder = BuildQueryBuilder<TEntity>(entityTypeId, options);

        var entries = await _client.GetEntries(queryBuilder, cancellationToken);

        ProcessContentfulErrors(entries);

        return entries.Items ?? [];
    }

    public async Task<IEnumerable<TEntity>> GetPaginatedEntities<TEntity>(string entityTypeId, IGetEntriesOptions options)
    {
        var limit = options?.Limit ?? 100;
        var queryBuilder = BuildQueryBuilder<TEntity>(entityTypeId, options)
            .Limit(limit)
            .Skip((options!.Page - 1) * limit);

        var entries = await _client.GetEntries(queryBuilder);

        ProcessContentfulErrors(entries);

        return entries.Items;
    }

    private void ProcessContentfulErrors<TEntity>(ContentfulCollection<TEntity> entries)
    {
        if (entries.Errors.Any())
        {
            _logger.LogError("Error retrieving entities from Contentful:\n{Errors}", entries.Errors.Select(CreateErrorString));
        }
    }

    private static string CreateErrorString(ContentfulError error)
    {
        var errorString = $"[{error.Details.Type}] {error.Details.Id}";
        if (error.Details.Id == error.SystemProperties.Id)
        {
            return errorString;
        }

        return errorString + " " + error.SystemProperties.Id;
    }
    public async Task<int> GetEntitiesCount<TEntity>(CancellationToken cancellationToken = default)
    {
        var queryBuilder = BuildQueryBuilder<TEntity>(GetContentTypeName<TEntity>(), null).Limit(0);

        var entries = await _client.GetEntries(queryBuilder, cancellationToken);

        ProcessContentfulErrors(entries);

        return entries.Total;

    }

    public async Task<IEnumerable<TEntity>> GetPaginatedEntities<TEntity>(IGetEntriesOptions options, CancellationToken cancellationToken = default)
        => await GetPaginatedEntities<TEntity>(GetContentTypeName<TEntity>(), options, cancellationToken);

    public async Task<TEntity?> GetEntityById<TEntity>(string id, int include = 2, CancellationToken cancellationToken = default)
    {
        var options = GetEntityByIdOptions(id, include);
        var entities = (await GetEntities<TEntity>(options, cancellationToken)).ToList();

        if (entities.Count > 1)
            throw new GetEntriesException($"Found more than 1 entity with id {id}");

        return entities.FirstOrDefault();
    }

    public GetEntriesOptions GetEntityByIdOptions(string id, int include = 2)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));

        //There exists a "GetEntry" option for the Contentful client, however the "Include"
        //option doesn't seem to have any effect there - it only seems to return the main parent entry
        //with links to children. This was proving rather useless, so I have used the "GetEntries" option here
        //instead.
        return new GetEntriesOptions(include, new[] {
            new ContentQuerySingleValue(){
                Field = "sys.id",
                Value = id
            }});
    }

    private static string GetContentTypeName<TEntity>()
    {
        var name = typeof(TEntity).Name;
        return name == "ContentSupportPage" ? name : name.FirstCharToLower();
    }

    private QueryBuilder<TEntity> BuildQueryBuilder<TEntity>(string contentTypeId, IGetEntriesOptions? options)
    {
        var queryBuilder = QueryBuilders.BuildQueryBuilder<TEntity>(contentTypeId, options);

        var shouldExcludeTestingContent = _hostEnvironment.IsProduction() || (!_automatedTestingOptions?.Contentful!?.IncludeTaggedContent ?? false);

        if (shouldExcludeTestingContent)
        {
            var tag = _automatedTestingOptions.Contentful?.Tag ?? null;
            queryBuilder = queryBuilder.FieldExcludes("metadata.tags.sys.id", [tag]);
        }

        return queryBuilder;
    }
}
