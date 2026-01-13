using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Caching.Interfaces;

namespace Dfe.PlanTech.Core.Caching;

[ExcludeFromCodeCoverage]
public class QuestionnaireCacher
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
