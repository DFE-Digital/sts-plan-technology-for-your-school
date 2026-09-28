namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IQuestionRepository
{
    Task<int> GetOrCreateQuestionIdAsync(string questionContentfulId, string questionText);
}
