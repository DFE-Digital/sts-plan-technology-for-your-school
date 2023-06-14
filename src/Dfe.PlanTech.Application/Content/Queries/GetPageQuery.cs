using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Caching.Models;
using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetPageQuery : ContentRetriever
{
    private readonly ISectionCacher _sectionCacher;
    
    public GetPageQuery(IContentRepository repository, ISectionCacher sectionCacher) : base(repository)
    {
        _sectionCacher = sectionCacher;
    }

    /// <summary>
    /// Fetches page from <see chref="IContentRepository"/> by slug
    /// </summary>
    /// <param name="slug">Slug for the Page</param>
    /// <returns>Page matching slug</returns>
    public async Task<Page> GetPageBySlug(string slug, CancellationToken cancellationToken = default)
    {
        var options = new GetEntitiesOptions(3, new[] { new ContentQueryEquals() { Field = "fields.slug", Value = slug } });
        var pages = await repository.GetEntities<Page>(options, cancellationToken);

        var page = pages.FirstOrDefault() ?? throw new Exception($"Could not find page with slug {slug}");

        if(page.DisplayTopicTitle){
            page = _sectionCacher.AddCurrentSectionTitle(page);
        }
        
        return page;
    }
}
