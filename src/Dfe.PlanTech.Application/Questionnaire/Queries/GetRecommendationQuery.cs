using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Common;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Questionnaire.Queries
{
    public class GetRecommendationQuery : ContentRetriever, IGetRecommendationQuery
    {

        public GetRecommendationQuery(IContentRepository repository) : base(repository)
        {
        }

        /// <summary>
        /// Returns recommendation chunks from contentful but only containing the system details ID and the header.
        /// </summary>
        public async Task<(IEnumerable<RecommendationChunk> Chunks, Pagination Pagination)> GetChunksByPage(int page, CancellationToken cancellationToken = default)
        {
            try
            {
                var totalEntries = await repository.GetEntitiesCount<RecommendationChunk>(cancellationToken);

                var options = new GetEntitiesOptions(include: 3) { Page = page };
                var result = await repository.GetPaginatedEntities<RecommendationChunk>(options, cancellationToken);

                return (result, new Pagination() { Page = page, Total = totalEntries });

            }
            catch (Exception ex)
            {
                throw new ContentfulDataUnavailableException("Error getting recommendation chunks from Contentful", ex);
            }
        }
    }
}
