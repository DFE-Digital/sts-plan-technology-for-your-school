using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Entries;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Repositories
{
    public class RecommendationChunkEntryRepository
    {
        private readonly ILogger<PageEntryRepository> _logger;
        private readonly ContentfulRepository _contentful;

        public RecommendationChunkEntryRepository(
            ILoggerFactory loggerFactory,
            ContentfulRepository contentfulBaseRepository
        )
        {
            _logger = loggerFactory.CreateLogger<PageEntryRepository>();
            _contentful = contentfulBaseRepository;
        }

        /// <summary>
        /// Returns recommendation chunks from contentful but only containing the system details ID and the header.
        /// </summary>
        public async Task<(IEnumerable<RecommendationChunkEntry> Chunks, PaginationModel Pagination)> GetChunksByPage(int page)
        {
            try
            {
                var totalEntries = await _contentful.GetEntriesCount<RecommendationChunkEntry>();

                var options = new GetEntriesOptions(include: 3) { Page = page };
                var result = await _contentful.GetPaginatedEntries<RecommendationChunkEntry>(options);

                return (result, new PaginationModel() { Page = page, Total = totalEntries });

            }
            catch (Exception ex)
            {
                throw new ContentfulDataUnavailableException("Error getting recommendation chunks from Contentful", ex);
            }
        }
    }
}
