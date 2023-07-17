namespace Dfe.PlanTech.Application.Submission.Interface;

using Dfe.PlanTech.Domain.Questions.Models;

public interface IGetQuestionQuery
{
    Task<Question?> GetQuestionBy(int questionId);
}