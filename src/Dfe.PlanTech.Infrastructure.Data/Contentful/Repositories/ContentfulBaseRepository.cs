using Contentful.Core;
using Contentful.Core.Models;
using Contentful.Core.Search;
using Dfe.PlanTech.Application.Options;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Infrastructure.Contentful;
using Dfe.PlanTech.Infrastructure.Contentful.Helpers;
using Dfe.PlanTech.Infrastructure.Contentful.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Repositories;

/// <summary>
/// Encapsulates ContentfulClient functionality, whilst abstracting through the IEntityRepository interface
/// </summary>
/// <see href="IEntityRepository"/>
public abstract class ContentfulBaseRepository
{
    private readonly IContentfulClient _client;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly AutomatedTestingOptions _automatedTestingOptions;
    private readonly ILogger<ContentfulBaseRepository> _logger;

    public ContentfulBaseRepository(ILoggerFactory loggerFactory, IContentfulClient client, IHostEnvironment hostEnvironment, IOptions<AutomatedTestingOptions> automatedTestingOptions)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _hostEnvironment = hostEnvironment;
        _automatedTestingOptions = automatedTestingOptions.Value;
        _client.ContentTypeResolver = new EntryResolver(loggerFactory.CreateLogger<EntryResolver>());
        _logger = loggerFactory.CreateLogger<ContentfulBaseRepository>();
    }

    public async Task<IEnumerable<TEntry>> GetEntries<TEntry>(CancellationToken cancellationToken = default)
        => await GetEntries<TEntry>(GetContentTypeName<TEntry>(), null, cancellationToken);

    public async Task<IEnumerable<TEntry>> GetEntries<TEntry>(IGetEntriesOptions options, CancellationToken cancellationToken = default)
        => await GetEntries<TEntry>(GetContentTypeName<TEntry>(), options, cancellationToken);

    public async Task<IEnumerable<TEntry>> GetEntries<TEntry>(string entityTypeId, IGetEntriesOptions? options, CancellationToken cancellationToken = default)
    {
        var queryBuilder = BuildQueryBuilder<TEntry>(entityTypeId, options);

        var entries = await _client.GetEntries(queryBuilder, cancellationToken);

        ProcessContentfulErrors(entries);

        return entries.Items ?? [];
    }

    public async Task<IEnumerable<TEntry>> GetPaginatedEntries<TEntry>(IGetEntriesOptions options)
        => await GetPaginatedEntries<TEntry>(GetContentTypeName<TEntry>(), options);

    public async Task<IEnumerable<TEntry>> GetPaginatedEntries<TEntry>(string entryTypeId, IGetEntriesOptions options)
    {
        var limit = options?.Limit ?? 100;
        var queryBuilder = BuildQueryBuilder<TEntry>(entryTypeId, options)
            .Limit(limit)
            .Skip((options!.Page - 1) * limit);

        var entries = await _client.GetEntries(queryBuilder);

        ProcessContentfulErrors(entries);

        return entries.Items;
    }

    public async Task<int> GetEntriesCount<TEntry>()
    {
        var queryBuilder = BuildQueryBuilder<TEntry>(GetContentTypeName<TEntry>(), null).Limit(0);
        var entries = await _client.GetEntries(queryBuilder);

        ProcessContentfulErrors(entries);

        return entries.Total;
    }

    public async Task<TEntry?> GetEntryById<TEntry>(string id, int include = 2)
    {
        var options = GetEntryByIdOptions(id, include);
        var entities = (await GetEntries<TEntry>(options)).ToList();

        if (entities.Count > 1)
            throw new GetEntriesException($"Found more than 1 entity with id {id}");

        return entities.FirstOrDefault();
    }

    public GetEntriesOptions GetEntryByIdOptions(string id, int include = 2)
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

    private QueryBuilder<TEntry> BuildQueryBuilder<TEntry>(string contentTypeId, IGetEntriesOptions? options)
    {
        var queryBuilder = QueryBuilders.BuildQueryBuilder<TEntry>(contentTypeId, options);

        var shouldExcludeTestingContent = _hostEnvironment.IsProduction() || (!_automatedTestingOptions?.Contentful!?.IncludeTaggedContent ?? false);

        if (shouldExcludeTestingContent)
        {
            var tag = _automatedTestingOptions?.Contentful?.Tag ?? null;
            queryBuilder = queryBuilder.FieldExcludes("metadata.tags.sys.id", [tag]);
        }

        return queryBuilder;
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

    private static string GetContentTypeName<TEntry>()
    {
        var name = typeof(TEntry).Name;
        return name == "ContentSupportPage" ? name : name.FirstCharToLower();
    }

    private void ProcessContentfulErrors<TEntry>(ContentfulCollection<TEntry> entries)
    {
        if (entries.Errors.Any())
        {
            var entryType = typeof(TEntry).Name;

            _logger.LogError("Error retrieving one or more {entryType} entries from Contentful:\n{Errors}", entryType, entries.Errors.Select(CreateErrorString));
        }
    }
}
