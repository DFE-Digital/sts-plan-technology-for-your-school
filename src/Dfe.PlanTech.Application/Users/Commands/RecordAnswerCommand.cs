using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Answers.Models;

namespace Dfe.PlanTech.Application.Users.Commands;

public class RecordAnswerCommand : IRecordAnswerCommand
{
    private readonly IAnswersDbContext _db;

    public RecordAnswerCommand(IAnswersDbContext db)
    {
        _db = db;
    }

    public async Task RecordAnswer(RecordAnswerDto recordAnswerDto)
    {
        CreateAnswerCommand createAnswerCommand = new CreateAnswerCommand(_db);
        await createAnswerCommand.CreateAnswer(recordAnswerDto);
    }
}