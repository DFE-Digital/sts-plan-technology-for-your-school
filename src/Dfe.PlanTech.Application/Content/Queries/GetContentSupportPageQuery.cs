using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;
public record GetContentSupportPageQuery(
    ICmsCache Cache,
    IContentRepository Repository,
    ILogger<GetContentSupportPageQuery> Logger) : IGetContentSupportPageQuery
{
    public async Task<ContentSupportPage?> GetContentSupportPage(string slug, CancellationToken cancellationToken = default)
    {
        try
        {
            var pages = await Cache.GetOrCreateAsync($"ContentSupportPage:{slug}", () => Repository.GetEntities<ContentSupportPage>(CreateGetEntityOptions(slug), cancellationToken)) ?? [];

            var page = pages.FirstOrDefault();

            return page;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching content support page {slug} from Contentful", slug);
            throw new ContentfulDataUnavailableException($"Could not retrieve content support page with slug {slug}", ex);
        }
    }

    private GetEntitiesOptions CreateGetEntityOptions(string slug) => new(10, [new ContentQueryEquals() { Field = "fields.Slug", Value = slug }]);
}
