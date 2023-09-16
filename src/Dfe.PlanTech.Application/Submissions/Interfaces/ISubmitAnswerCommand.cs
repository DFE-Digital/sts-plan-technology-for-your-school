using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Submissions.Interfaces;

public interface ISubmitAnswerCommand
{
    public Task<int> SubmitAnswer(SubmitAnswerDto submitAnswerDto, string sectionId, string sectionName);
}