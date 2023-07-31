using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Questionnaire.Queries
{
    public class GetSectionQuery : ContentRetriever
    {
        public GetSectionQuery(IContentRepository repository) : base(repository) { }

        public async Task<Section?> GetSectionById(string sectionId, CancellationToken cancellationToken = default)
        {
            return await repository.GetEntityById<Section>(sectionId, 3, cancellationToken);
        }
    }
}