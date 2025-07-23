using Contentful.Core.Models;
using Dfe.PlanTech.Core.Content.Options;
using Dfe.PlanTech.Core.Content.Queries;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Repositories;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Data.Contentful.Persistence
{
    public class SectionWorkflow
    {
        public const string ExceptionMessageContentful = "Error getting navigation links from Contentful";
        public const string SlugFieldPath = "fields.interstitialPage.fields.slug";

        private readonly ILogger<SectionWorkflow> _logger;

        private readonly ContentfulRepository _contentful;

        public SectionWorkflow(
            ILoggerFactory loggerFactory,
            ContentfulRepository contentfulBaseRepository
        )
        {
            _logger = loggerFactory.CreateLogger<SectionWorkflow>();
            _contentful = contentfulBaseRepository;
        }

        public async Task<QuestionnaireSectionEntry?> GetSectionBySlug(string sectionSlug)
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
                var sections = await _contentful.GetEntriesAsync<QuestionnaireSectionEntry>(options);
                return sections.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new ContentfulDataUnavailableException($"Error getting section with slug {sectionSlug} from Contentful", ex);
            }
        }

        public async Task<QuestionnaireSectionEntry?> GetSectionById(string contentId)
        {
            try
            {
                return await _contentful.GetEntryById<QuestionnaireSectionEntry?>(contentId);
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
        public async Task<IEnumerable<Section>> GetSectionsForTraversalAsync()
        {
            var summariseAnswer = (QuestionnaireAnswerEntry answer) =>
            {
                return new Entry<QuestionnaireQuestionEntry>()
                {
                    Fields = new QuestionnaireQuestionEntry
                    {
                        Text = answer.Text,
                        SystemProperties = answer.SystemProperties,
                        NextQuestion = answer.NextQuestion is not null
                        ? new Entry<QuestionnaireQuestionEntry>()
                        {
                            SystemProperties = answer.NextQuestion.SystemProperties,
                        }
                        : null
                    }
                };
            };

            var summariseQuestion = (QuestionnaireQuestionEntry question) =>
            {
                return new QuestionnaireQuestionEntry
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
