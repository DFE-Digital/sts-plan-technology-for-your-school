using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Web.Exceptions;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetPageQuery : ContentRetriever, IGetPageQuery
{
    private readonly IQuestionnaireCacher _cacher;

    readonly string _getEntityEnvVariable = Environment.GetEnvironmentVariable("CONTENTFUL_GET_ENTITY_INT") ?? "4";

    public GetPageQuery(IQuestionnaireCacher cacher, IContentRepository repository) : base(repository)
    {
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
        catch (Exception e)
        {
            throw new ContentfulDataUnavailableException($"Could not retrieve page with slug {slug}", e);
        }
    }
}