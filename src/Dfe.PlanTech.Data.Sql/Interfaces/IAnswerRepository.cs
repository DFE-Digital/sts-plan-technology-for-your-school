namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IAnswerRepository
{
    Task<int> GetOrCreateAnswerIdAsync(string answerContentfulId, string answerText);
}
