using Dfe.PlanTech.Domain.Caching.Models;

namespace Dfe.PlanTech.Domain.Caching.Interfaces;

public interface IQuestionnaireCacher
{
    public QuestionnaireCache? Cached { get; }

    public void SaveCache(QuestionnaireCache cache);
}
