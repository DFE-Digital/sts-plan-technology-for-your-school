using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Options;
using Dfe.PlanTech.Data.Contentful.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class RecommendationWorkflow(
    IContentfulRepository contentfulRepository
) : IRecommendationWorkflow
{
    private readonly IContentfulRepository _contentfulRepository = contentfulRepository ?? throw new ArgumentNullException(nameof(contentfulRepository));

    public async Task<int> GetRecommendationChunkCount(int page)
    {
        return await _contentfulRepository.GetEntriesCount<RecommendationChunkEntry>();
    }

    public Task<IEnumerable<RecommendationChunkEntry>> GetPaginatedRecommendationEntries(int page)
    {
        var options = new GetEntriesOptions(include: 3) { Page = page };
        return _contentfulRepository.GetPaginatedEntriesAsync<RecommendationChunkEntry>(options);
    }
}
