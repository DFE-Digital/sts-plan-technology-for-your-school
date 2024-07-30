using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;

namespace Dfe.PlanTech.Application.Caching.Models;

public class QuestionnaireCacher : IQuestionnaireCacher
{
    private const string CACHE_KEY = "QuestionnaireCache";

    private readonly ICacher _cacher;

    public QuestionnaireCacher(ICacher cacher)
    {
        _cacher = cacher;
    }

    public QuestionnaireCache? Cached => _cacher.Get(CACHE_KEY, () => new QuestionnaireCache());

    public void SaveCache(QuestionnaireCache cache) => _cacher.Set(CACHE_KEY, TimeSpan.FromHours(1), cache);
}
