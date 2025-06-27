using Dfe.PlanTech.Infrastructure.Data.Contentful.Entries;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Repositories
{
    public class NavigationLinkEntryRepository
    {
        public const string ExceptionMessageContentful = "Error getting navigation links from Contentful";

        private readonly ILogger<NavigationLinkEntryRepository> _logger;

        private readonly ContentfulContext _contentful;

        public NavigationLinkEntryRepository(
            ILoggerFactory loggerFactory,
            ContentfulContext contentfulBaseRepository
        )
        {
            _logger = loggerFactory.CreateLogger<NavigationLinkEntryRepository>();
            _contentful = contentfulBaseRepository;
        }

        public async Task<IEnumerable<NavigationLinkEntry>> GetNavigationLinks()
        {
            try
            {
                return await _contentful.GetEntries<NavigationLinkEntry>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessageContentful);
                return [];
            }
        }

        public async Task<NavigationLinkEntry?> GetLinkById(string contentId)
        {
            try
            {
                return await _contentful.GetEntryById<NavigationLinkEntry?>(contentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessageContentful);
                return null;
            }
        }
    }
}
