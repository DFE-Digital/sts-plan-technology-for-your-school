using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IQuestionRepository
{
    Task<List<QuestionEntity>> GetQuestionsByContenfulRef(IEnumerable<string> sectionQuestionRefs);

    Task<int> GetOrCreateQuestionIdAsync(string questionContentfulId, string questionText);
}
