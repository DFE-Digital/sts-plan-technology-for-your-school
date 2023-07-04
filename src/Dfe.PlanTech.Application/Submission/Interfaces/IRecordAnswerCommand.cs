using Dfe.PlanTech.Domain.Answers.Models;

namespace Dfe.PlanTech.Application.Submission.Interfaces;

public interface IRecordAnswerCommand
{
    Task RecordAnswer(RecordAnswerDto recordAnswerDto);
}
