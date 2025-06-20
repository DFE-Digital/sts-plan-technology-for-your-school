using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;

namespace Dfe.PlanTech.Application.Questionnaire.Queries;

public class GetSectionQuery : ContentRetriever, IGetSectionQuery
{
    public const string SlugFieldPath = "fields.interstitialPage.fields.slug";

    public GetSectionQuery(IContentRepository repository) : base(repository)
    {
    }

    public async Task<Section?> GetSectionBySlug(string sectionSlug, CancellationToken cancellationToken = default)
    {
        var options = new GetEntitiesOptions()
        {
            Queries =
            [
                new ContentQuerySingleValue()
                {
                    Field = SlugFieldPath,
                    Value = sectionSlug
                },
                new ContentQuerySingleValue()
                {
                    Field = "fields.interstitialPage.sys.contentType.sys.id",
                    Value = "page"
                }
            ]
        };

        try
        {
            var sections = await repository.GetEntities<Section>(options, cancellationToken);
            return sections.FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new ContentfulDataUnavailableException($"Error getting section with slug {sectionSlug} from Contentful", ex);
        }
    }

    /// <summary>
    /// Returns sections from contentful but only containing system details, question and answer text
    /// </summary>
    public async Task<IEnumerable<Section?>> GetAllSections(CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new GetEntitiesOptions(include: 3);
            var sections = await repository.GetEntities<Section>(options, cancellationToken);
            return sections.Select(MapSection);
        }
        catch (Exception ex)
        {
            throw new ContentfulDataUnavailableException("Error getting sections from Contentful", ex);
        }
    }

    /// <summary>
    /// Returns a mapped section only containing the text and ids required to see the paths through a section.
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
                Answers = question.Answers.Select(MapAnswer).ToList()
            }).ToList()
        };
    }

    /// <summary>
    /// Removes details from nextQuestion other than system details to avoid infinite reference loops
    /// </summary>
    private Answer MapAnswer(Answer answer)
    {
        return new Answer()
        {
            Text = answer.Text,
            Sys = answer.Sys,
            NextQuestion = answer.NextQuestion != null ? new Question() { Sys = answer.NextQuestion.Sys, } : null
        };
    }
}
