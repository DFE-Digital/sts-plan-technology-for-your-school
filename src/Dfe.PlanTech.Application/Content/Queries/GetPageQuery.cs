using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetPageQuery : ContentRetriever
{
    public GetPageQuery(IContentRepository repository) : base(repository)
    {
    }

    public async Task<Page> GetPageBySlug(string slug)
    {
        var filters = new[] { new ContentQueryEquals() { Field = "fields.slug", Value = slug } };

        var pages = await repository.GetEntities<Page>(filters);

        var page = pages.FirstOrDefault();

        if (page == null) throw new Exception($"Could not find page with slug {slug}");

        return page;
    }
}
