using Dfe.PlanTech.Core.Content.Options;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Data.Contentful.Interfaces;

namespace Dfe.PlanTech.Application.Workflows
{
    public class RecommendationWorkflow(
        IContentfulRepository contentfulRepository
    )
    {
        private readonly IContentfulRepository _contentfulRepository = contentfulRepository ?? throw new ArgumentNullException(nameof(contentfulRepository));

        public async Task<int> GetRecommendationChunkCount(int page)
        {
            return await _contentfulRepository.GetEntriesCount<RecommendationChunkEntry>();
        }

        public async Task<IEnumerable<CmsRecommendationChunkDto>> GetPaginatedRecommendationEntries(int page)
        {
            var options = new GetEntriesOptions(include: 3) { Page = page };
            var entries = await _contentfulRepository.GetPaginatedEntriesAsync<RecommendationChunkEntry>(options);
            return entries.Select(e => e.AsDto());
        }
    }
}
