using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Application.Caching.Models;

public class QuestionnaireCacher : BaseCacher, IQuestionnaireCacher
{
    private const string CACHE_KEY = "QuestionnaireCache";

    private readonly Func<QuestionnaireCache> CreateNewCache = () => new QuestionnaireCache();

    public QuestionnaireCacher(ICacher cacher, IHttpContextAccessor httpContextAccessor) : base(CACHE_KEY, cacher, httpContextAccessor)
    {
    }

    public Task<QuestionnaireCache?> Cached => _cacher.GetAsync(KeyForUser, CreateNewCache);

    public Task SaveCache(QuestionnaireCache cache) => _cacher.SetAsync(KeyForUser, cache);

}