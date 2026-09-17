using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IQuestionRepository
{
    Task<List<QuestionEntity>> GetQuestionsForSection(QuestionnaireSectionEntry section);

    Task<int> GetOrCreateQuestionIdAsync(string questionContentfulId, string questionText);
}
