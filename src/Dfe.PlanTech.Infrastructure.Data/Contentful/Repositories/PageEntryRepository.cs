using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Entries;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Repositories
{
    public class PageEntryRepository
    {
        public const string ExceptionMessageContentful = "Error getting navigation links from Contentful";
        private const int NumberOfReferenceLevels = 4;

        private readonly ILogger<PageEntryRepository> _logger;
        private readonly ContentfulBaseRepository _contentful;

        public PageEntryRepository(
            ILoggerFactory loggerFactory,
            ContentfulBaseRepository contentfulBaseRepository
        )
        {
            _logger = loggerFactory.CreateLogger<PageEntryRepository>();
            _contentful = contentfulBaseRepository;
        }

        public Task<PageEntry?> GetPageByIdAsync(string id)
        {
            try
            {
                return _contentful.GetEntryById<PageEntry?>(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching page with ID '{id}' from Contentful", id);
                throw new ContentfulDataUnavailableException($"Could not retrieve page with ID '{id}'", ex);
            }
        }

        public async Task<PageEntry?> GetPageBySlugAsync(string slug)
        {
            var options = CreateGetEntityOptions(slug);

            try
            {
                var pages = await _contentful.GetEntries<PageEntry?>(options);
                return pages.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching page '{slug}' from Contentful", slug);
                throw new ContentfulDataUnavailableException($"Could not retrieve page with slug '{slug}'", ex);
            }
        }

        private GetEntriesOptions CreateGetEntityOptions(string slug) =>
            new(NumberOfReferenceLevels, [new ContentQuerySingleValue() { Field = "fields.slug", Value = slug }]);
    }
}
