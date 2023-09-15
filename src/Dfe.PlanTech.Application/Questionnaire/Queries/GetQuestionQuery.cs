using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;

namespace Dfe.PlanTech.Application.Questionnaire.Queries;

public class GetQuestionQuery : ContentRetriever
{
    private readonly IQuestionnaireCacher _cacher;

    public GetQuestionQuery(IQuestionnaireCacher cacher, IContentRepository repository) : base(repository)
    {
        _cacher = cacher;
    }

    public async Task<Question?> GetQuestionById(string id, CancellationToken cancellationToken = default)
    {
        var question = await repository.GetEntityById<Question>(id, 3, cancellationToken);

        return question;
    }

    public Task<Question?> GetQuestionById(string id, string? section, CancellationToken cancellationToken = default)
    {
        if (section != null)
        {
            UpdateSectionTitle(section);
        }

        return GetQuestionById(id, cancellationToken);
    }

    public async Task<QuestionWithSectionDto> GetQuestionBySlug(string sectionSlug, string questionSlug, CancellationToken cancellationToken = default)
    {
        var options = new GetEntitiesOptions()
        {
            Queries = new[] {
                new ContentQueryEquals(){
                    Field = "fields.interstitialPage.fields.slug",
                    Value = sectionSlug
                },
                new ContentQueryEquals(){
                    Field="fields.interstitialPage.sys.contentType.sys.id",
                    Value="page"
                }
            }
        };

        var section = (await repository.GetEntities<Section>(options, cancellationToken)).FirstOrDefault() ??
                    throw new KeyNotFoundException($"Unable to find section with slug {sectionSlug}");

        var question = Array.Find(section.Questions, question => question.Slug == questionSlug) ??
                        throw new KeyNotFoundException($"Unable to find question with slug {questionSlug} under section {sectionSlug}");

        return new QuestionWithSectionDto(question,
                                            SectionId: section.Sys.Id,
                                            SectionName: section.Name,
                                            SectionSlug: section.InterstitialPage.Slug);
    }

    private void UpdateSectionTitle(string section)
    {
        var cached = _cacher.Cached ?? new QuestionnaireCache();
        cached = cached with { CurrentSectionTitle = section };

        _cacher.SaveCache(cached);
    }
}
