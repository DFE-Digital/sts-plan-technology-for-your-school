using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Web.Exceptions;

namespace Dfe.PlanTech.Application.Questionnaire.Queries;

public class GetSectionQuery : ContentRetriever, IGetSectionQuery
{
    public const string SlugFieldPath = "fields.interstitialPage.fields.slug";

    public GetSectionQuery(IContentRepository repository) : base(repository) { }

    public Task<Section?> GetSectionById(string sectionId, CancellationToken cancellationToken = default)
    => repository.GetEntityById<Section>(sectionId, 3, cancellationToken);

    public async Task<Section?> GetSectionBySlug(string sectionSlug, CancellationToken cancellationToken = default)
    {
        var options = new GetEntitiesOptions()
        {
            Queries = new[] {
                    new ContentQueryEquals(){
                        Field = SlugFieldPath,
                        Value = sectionSlug
                    },
                    new ContentQueryEquals(){
                    Field="fields.interstitialPage.sys.contentType.sys.id",
                    Value="page"
                    }
                }
        };

        try
        {
            return (await repository.GetEntities<Section>(options, cancellationToken)).FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new ContentfulDataUnavailableException($"Error getting section with slug {sectionSlug}", ex);
        }
    }
}
