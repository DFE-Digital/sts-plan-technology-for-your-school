using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;

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

    public async Task<Question?> GetQuestionById(string id, string? section, CancellationToken cancellationToken = default)
    {
        if (section != null)
        {
            await UpdateSectionTitle(section);
        }

        return await GetQuestionById(id, cancellationToken);
    }

    private async Task UpdateSectionTitle(string section)
    {
        var cached = (await _cacher.Cached) ?? new QuestionnaireCache();
        cached = cached with { CurrentSectionTitle = section };

        await _cacher.SaveCache(cached);
    }
}
