using Dfe.PlanTech.Domain.Caching.Models;

namespace Dfe.PlanTech.Application.Caching.Interfaces;

public interface IQuestionnaireCacher
{
    public Task<QuestionnaireCache?> Cached { get; }

    public Task SaveCache(QuestionnaireCache cache);
}
