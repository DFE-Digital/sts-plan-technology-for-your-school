using Dfe.PlanTech.Domain.Answers.Models;

namespace Dfe.PlanTech.Application.Users.Interfaces;

public interface IRecordAnswerCommand
{
    Task RecordAnswer(RecordAnswerDto recordAnswerDto);
}
