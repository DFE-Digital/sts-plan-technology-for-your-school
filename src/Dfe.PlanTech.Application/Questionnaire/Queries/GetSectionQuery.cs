using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;

namespace Dfe.PlanTech.Application.Questionnaire.Queries
{
    public class GetSectionQuery : ContentRetriever
    {
        public GetSectionQuery(IContentRepository repository) : base(repository) { }

        public async Task<Section?> GetSectionById(string sectionId, CancellationToken cancellationToken = default)
        {
            return await repository.GetEntityById<Section>(sectionId, 3, cancellationToken);
        }


        public async Task<Section?> GetSectionByName(string sectionName, CancellationToken cancellationToken = default)
        {
            var query = new GetEntitiesOptions(3, new[] {
                new ContentQueryEquals(){
                    Field = "fields.name",
                    Value = sectionName
                }
            });

            return (await repository.GetEntities<Section>(query, cancellationToken)).FirstOrDefault();
        }
    }
}