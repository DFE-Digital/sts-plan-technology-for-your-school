using AutoMapper;
using AutoMapper.QueryableExtensions;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Infrastructure.Application.Models;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetPageQuery : ContentRetriever, IGetPageQuery
{
    private readonly ICmsDbContext _db;
    private readonly IQuestionnaireCacher _cacher;
    private readonly IMapper _mapperConfiguration;

    readonly string _getEntityEnvVariable = Environment.GetEnvironmentVariable("CONTENTFUL_GET_ENTITY_INT") ?? "4";

    public GetPageQuery(IMapper mapperConfiguration, ICmsDbContext db, IQuestionnaireCacher cacher, IContentRepository repository) : base(repository)
    {
        _mapperConfiguration = mapperConfiguration;
        _db = db;
        _cacher = cacher;
    }

    /// <summary>
    /// Fetches page from <see chref="IContentRepository"/> by slug
    /// </summary>
    /// <param name="slug">Slug for the Page</param>
    /// <returns>Page matching slug</returns>
    public async Task<Page> GetPageBySlug(string slug, CancellationToken cancellationToken = default)
    {
        try
        {
            var buttons = await _db.ToListAsync(_db.Buttons.ProjectTo<Button>(_mapperConfiguration.ConfigurationProvider), cancellationToken);
            var page = await _db.GetPageBySlug(slug, cancellationToken);

            if (page == null) return await GetFromContentful(slug, cancellationToken);

            var mapped = _mapperConfiguration.Map<PageDbEntity, Page>(page);

            return mapped;
        }
        catch (Exception e)
        {
            throw new ContentfulDataUnavailableException($"Could not retrieve page with slug {slug}", e);
        }
    }

    private async Task<Page> GetFromContentful(string slug, CancellationToken cancellationToken)
    {
        if (int.TryParse(_getEntityEnvVariable, out int getEntityValue))
        {

            var options = new GetEntitiesOptions(getEntityValue,
                new[] { new ContentQueryEquals() { Field = "fields.slug", Value = slug } });
            var pages = await repository.GetEntities<Page>(options, cancellationToken);

            var page = pages.FirstOrDefault() ?? throw new KeyNotFoundException($"Could not find page with slug {slug}");

            if (page.DisplayTopicTitle)
            {
                var cached = _cacher.Cached!;
                page.SectionTitle = cached.CurrentSectionTitle;
            }

            return page;
        }
        else
        {
            throw new FormatException($"Could not parse CONTENTFUL_GET_ENTITY_INT environment variable to int. Value: {_getEntityEnvVariable}");
        }
    }
}