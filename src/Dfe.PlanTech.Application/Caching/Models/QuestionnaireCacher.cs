using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;

namespace Dfe.PlanTech.Application.Caching.Models;

public class QuestionnaireCacher : IQuestionnaireCacher
{
    private const string CACHE_KEY = "QuestionnaireCache";

    private readonly ICacher _cacher;
    private readonly Func<QuestionnaireCache> CreateNewCache = () => new QuestionnaireCache();

    public QuestionnaireCacher(ICacher cacher)
    {
        _cacher = cacher;
    }

    public Task<QuestionnaireCache?> Cached => _cacher.GetAsync(CACHE_KEY, CreateNewCache);

    public Task SaveCache(QuestionnaireCache cache) => _cacher.SetAsync(CACHE_KEY, cache);

}