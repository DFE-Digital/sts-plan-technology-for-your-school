using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Entries;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Queries;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Repositories
{
    public class SectionEntryRepository
    {
        public const string ExceptionMessageContentful = "Error getting navigation links from Contentful";
        public const string SlugFieldPath = "fields.interstitialPage.fields.slug";

        private readonly ILogger<SectionEntryRepository> _logger;

        private readonly ContentfulContext _contentful;

        public SectionEntryRepository(
            ILoggerFactory loggerFactory,
            ContentfulContext contentfulBaseRepository
        )
        {
            _logger = loggerFactory.CreateLogger<SectionEntryRepository>();
            _contentful = contentfulBaseRepository;
        }

        public async Task<SectionEntry?> GetSectionBySlug(string sectionSlug)
        {
            var options = new GetEntriesOptions()
            {
                Queries =
                [
                    new ContentfulQuerySingleValue()
                    {
                        Field = SlugFieldPath,
                        Value = sectionSlug
                    },
                    new ContentfulQuerySingleValue()
                    {
                        Field = "fields.interstitialPage.sys.contentType.sys.id",
                        Value = "page"
                    }
                ]
            };

            try
            {
                var sections = await _contentful.GetEntries<SectionEntry>(options);
                return sections.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new ContentfulDataUnavailableException($"Error getting section with slug {sectionSlug} from Contentful", ex);
            }
        }

        public async Task<SectionEntry?> GetSectionById(string contentId)
        {
            try
            {
                return await _contentful.GetEntryById<SectionEntry?>(contentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessageContentful);
                return null;
            }
        }

        /// <summary>
        /// Returns sections from contentful but only containing system details, question and answer text.
        /// Sections only contain the text and IDs required to see the paths through a section.
        /// Answers have details removed nextQuestion other than system details to avoid infinite reference loops.
        /// </summary>
        public async Task<IEnumerable<SectionEntry>> GetSectionsForTraversalAsync()
        {
            var summariseAnswer = (AnswerEntry answer) =>
            {
                return new AnswerEntry()
                {
                    Text = answer.Text,
                    Sys = answer.Sys,
                    NextQuestion = answer.NextQuestion != null
                        ? new QuestionEntry()
                        {
                            Sys = answer.NextQuestion.Sys,
                        }
                        : null
                };
            };

            var summariseQuestion = (QuestionEntry question) =>
            {
                return new QuestionEntry
                {
                    Sys = question.Sys,
                    Text = question.Text,
                    Answers = question.Answers.Select(summariseAnswer).ToList()
                };
            };

            var summariseSection = (SectionEntry section) =>
            {
                return new SectionEntry
                {
                    Name = section.Name,
                    Sys = section.Sys,
                    Questions = section.Questions.Select(summariseQuestion).ToList()
                };
            };

            try
            {
                var options = new GetEntriesOptions(include: 3);
                var sections = await _contentful.GetEntries<SectionEntry>(options);
                return sections.Select(summariseSection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessageContentful);
                return [];
            }
        }
    }
}
