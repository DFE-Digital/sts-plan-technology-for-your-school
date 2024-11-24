using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

/// <summary>
/// Retrieves Sections with question and answer text from the CMS
/// </summary>
public class GetSectionsQuery : ContentRetriever, IGetSectionsQuery
{
    private readonly ILogger<GetSectionsQuery> _logger;
    private readonly ICmsCache _cache;

    public GetSectionsQuery(ILogger<GetSectionsQuery> logger, IContentRepository repository, ICmsCache cache) : base(repository)
    {
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// Returns sections from contentful but only containing system details, question and answer text
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Section?>> GetSections(CancellationToken cancellationToken = default)
    {
        try
        {
            var sections = await _cache.GetOrCreateAsync("Sections", async () =>
            {
                var options = new GetEntitiesOptions(include: 2);
                var sections = await repository.GetEntities<Section>(options, cancellationToken);
                return sections.Select(MapSection);
            }) ?? [];
            return sections;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sections from Contentful");
            return [];
        }
    }

    /// <summary>
    /// Returns a new section only containing question and answer text and Id of the next question
    /// </summary>
    private Section MapSection(Section section)
    {
        return new Section()
        {
            Name = section.Name,
            Sys = section.Sys,
            Questions = section.Questions.Select(question => new Question()
            {
                Sys = question.Sys,
                Text = question.Text,
                Answers = question.Answers.Select(answer => new Answer()
                {
                    Text = answer.Text,
                    Sys = answer.Sys,
                    NextQuestion = answer.NextQuestion != null
                        ? new Question() { Sys = answer.NextQuestion.Sys, }
                        : null,
                }).ToList(),
            }).ToList()
        };
    }
}
