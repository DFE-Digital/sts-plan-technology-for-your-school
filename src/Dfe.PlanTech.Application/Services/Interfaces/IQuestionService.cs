using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Services.Interfaces
{
    public interface IQuestionService
    {
        Task<QuestionnaireQuestionEntry?> GetNextUnansweredQuestion(int establishmentId, QuestionnaireSectionEntry section);
    }
}
