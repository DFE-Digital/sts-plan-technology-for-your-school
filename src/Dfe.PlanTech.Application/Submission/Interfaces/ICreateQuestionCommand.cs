using Dfe.PlanTech.Domain.Questions.Models;

namespace Dfe.PlanTech.Application.Submission.Interfaces;

public interface ICreateQuestionCommand
{
    Task<int> CreateQuestion(RecordQuestionDto recordQuestionDto);
}