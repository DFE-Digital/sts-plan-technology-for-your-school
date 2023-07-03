using Dfe.PlanTech.Domain.Answers.Models;

namespace Dfe.PlanTech.Application.Persistence.Interfaces;

public interface ICreateAnswerCommand
{
    Task<int> CreateAnswer(RecordAnswerDto recordAnswerDto);
}