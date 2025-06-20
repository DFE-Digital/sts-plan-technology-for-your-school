using Dfe.PlanTech.Domain.Common;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Interfaces
{
    public interface IGetRecommendationQuery
    {
        public Task<(IEnumerable<RecommendationChunk> Chunks, Pagination Pagination)> GetChunksByPage(int page, CancellationToken cancellationToken = default);

    }
}
