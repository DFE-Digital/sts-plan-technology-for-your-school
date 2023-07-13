namespace Dfe.PlanTech.Application.Submission.Interface;

using Dfe.PlanTech.Domain.Answers.Models;

public interface IGetAnswerQuery
{
    Task<Answer?> GetAnswerBy(int answerId);
}