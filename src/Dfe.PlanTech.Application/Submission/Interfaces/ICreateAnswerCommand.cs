using Dfe.PlanTech.Domain.Answers.Models;

namespace Dfe.PlanTech.Application.Submission.Interfaces;

public interface ICreateAnswerCommand
{
    Task<int> CreateAnswer(RecordAnswerDto recordAnswerDto);
}