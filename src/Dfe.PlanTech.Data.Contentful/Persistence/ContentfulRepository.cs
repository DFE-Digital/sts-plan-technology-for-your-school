using Contentful.Core;
using Contentful.Core.Models;
using Contentful.Core.Search;
using Dfe.PlanTech.Core.Contentful.Options;
using Dfe.PlanTech.Core.Contentful.Queries;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Options;
using Dfe.PlanTech.Data.Contentful.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Data.Contentful.Persistence;

/// <summary>
/// Encapsulates ContentfulClient functionality, whilst abstracting through the IEntityRepository interface
/// </summary>
public class ContentfulRepository : IContentfulRepository
{
    private readonly ILogger<IContentfulRepository> _logger;
    private readonly IContentfulClient _client;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly AutomatedTestingOptions _automatedTestingOptions;

    public ContentfulRepository(
        ILogger<ContentfulRepository> logger,
        IContentfulClient client,
        IHostEnvironment hostEnvironment,
        IOptions<AutomatedTestingOptions> automatedTestingOptions
    )
    {
        _logger = logger;
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
        _automatedTestingOptions = automatedTestingOptions?.Value ?? throw new ArgumentNullException(nameof(automatedTestingOptions));

        _client.ContentTypeResolver = new EntryResolver();
    }

    public async Task<TEntry?> GetEntryByIdAsync<TEntry>(string id, int include = 2)
    {
        var options = GetEntryByIdOptions(id, include);
        var entities = (await GetEntriesAsync<TEntry>(options)).ToList();

        if (entities.Count > 1)
            throw new GetEntriesException($"Found more than 1 entity with id {id}");

        return entities.FirstOrDefault();
    }

    public async Task<IEnumerable<TEntry>> GetEntriesAsync<TEntry>()
        => await GetEntriesAsync<TEntry>(ContentfulContentTypeHelper.GetContentTypeName<TEntry>(), null);

    public async Task<IEnumerable<TEntry>> GetEntriesAsync<TEntry>(GetEntriesOptions options)
        => await GetEntriesAsync<TEntry>(ContentfulContentTypeHelper.GetContentTypeName<TEntry>(), options);

    public async Task<IEnumerable<TEntry>> GetEntriesAsync<TEntry>(string entityTypeId, GetEntriesOptions? options)
    {
        var queryBuilder = BuildQueryBuilder<TEntry>(entityTypeId, options);

        var entries = await _client.GetEntries(queryBuilder);
        ProcessContentfulErrors(entries);

        return entries.Items ?? [];
    }

    public async Task<IEnumerable<TEntry>> GetPaginatedEntriesAsync<TEntry>(GetEntriesOptions options)
        => await GetPaginatedEntries<TEntry>(ContentfulContentTypeHelper.GetContentTypeName<TEntry>(), options);

    public async Task<IEnumerable<TEntry>> GetPaginatedEntries<TEntry>(string entryTypeId, GetEntriesOptions options)
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
        var queryBuilder = BuildQueryBuilder<TEntry>(ContentfulContentTypeHelper.GetContentTypeName<TEntry>(), null).Limit(0);
        var entries = await _client.GetEntries(queryBuilder);

        ProcessContentfulErrors(entries);

        return entries.Total;
    }

    public GetEntriesOptions GetEntryByIdOptions(string id, int include = 2)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));

        // There exists a "GetEntry" option for the Contentful client, however the "Include"
        // option doesn't seem to have any effect there - it only seems to return the main parent entry
        // with links to children. This was proving rather useless, so I have used the "GetEntries" option here
        // instead.
        return new GetEntriesOptions(include, [
            new ContentfulQuerySingleValue(){
                Field = "sys.id",
                Value = id
            }]);
    }

    private QueryBuilder<TEntry> BuildQueryBuilder<TEntry>(string contentfulContentTypeId, GetEntriesOptions? options)
    {
        var queryBuilder = QueryBuilders.BuildQueryBuilder<TEntry>(contentfulContentTypeId, options);

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

    private void ProcessContentfulErrors<TEntry>(ContentfulCollection<TEntry> entries)
    {
        if (entries.Errors.Any())
        {
            var entryType = typeof(TEntry).Name;

            _logger.LogError("Error retrieving one or more {entryType} entries from Contentful:\n{Errors}", entryType, entries.Errors.Select(CreateErrorString));
        }
    }

    GetEntriesOptions IContentfulRepository.GetEntryByIdOptions(string id, int include)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));

        return new GetEntriesOptions(include, [
            new ContentfulQuerySingleValue(){
                Field = "sys.id",
                Value = id
            }]);
    }
}
