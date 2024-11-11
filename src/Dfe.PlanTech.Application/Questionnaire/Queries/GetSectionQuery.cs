using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;

namespace Dfe.PlanTech.Application.Questionnaire.Queries;

public class GetSectionQuery : ContentRetriever, IGetSectionQuery
{
    public const string SlugFieldPath = "fields.interstitialPage.fields.slug";
    private readonly ICmsCache _cache;

    public GetSectionQuery(IContentRepository repository, ICmsCache cache) : base(repository)
    {
        _cache = cache;
    }

    public async Task<Section?> GetSectionBySlug(string sectionSlug, CancellationToken cancellationToken = default)
    {
        var options = new GetEntitiesOptions()
        {
            Queries =
            [
                new ContentQueryEquals()
                {
                    Field = SlugFieldPath,
                    Value = sectionSlug
                },
                new ContentQueryEquals()
                {
                    Field = "fields.interstitialPage.sys.contentType.sys.id",
                    Value = "page"
                }
            ]
        };

        try
        {
            var sections = await _cache.GetOrCreateAsync($"Section:{sectionSlug}", () => repository.GetEntities<Section>(options, cancellationToken)) ?? [];
            return sections.FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new ContentfulDataUnavailableException($"Error getting section with slug {sectionSlug} from Contentful", ex);
        }
    }
}
