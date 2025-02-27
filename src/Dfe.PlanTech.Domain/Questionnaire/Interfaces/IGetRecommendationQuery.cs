using Dfe.PlanTech.Domain.Common;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces
{
    public interface IGetRecommendationQuery
    {
        public Task<(IEnumerable<RecommendationChunk> Chunks, Pagination Pagination)> GetChunksByPage(int page, CancellationToken cancellationToken = default);

    }
}
