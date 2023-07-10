using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Answers.Models;

namespace Dfe.PlanTech.Application.Submission.Commands;

public class RecordAnswerCommand : IRecordAnswerCommand
{
    private readonly IPlanTechDbContext _db;

    public RecordAnswerCommand(IPlanTechDbContext db)
    {
        _db = db;
    }

    public async Task<int> RecordAnswer(RecordAnswerDto recordAnswerDto)
    {
        CreateAnswerCommand createAnswerCommand = new CreateAnswerCommand(_db);
        return await createAnswerCommand.CreateAnswer(recordAnswerDto);
    }
}