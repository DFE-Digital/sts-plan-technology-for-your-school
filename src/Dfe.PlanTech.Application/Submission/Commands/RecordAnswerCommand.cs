using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Answers.Models;

namespace Dfe.PlanTech.Application.Submission.Commands;

public class RecordAnswerCommand : IRecordAnswerCommand
{
    private readonly IAnswersDbContext _db;
    private readonly ICreateAnswerCommand _createAnswerCommand;

    public RecordAnswerCommand(IAnswersDbContext db, ICreateAnswerCommand createAnswerCommand)
    {
        _db = db;
        _createAnswerCommand = createAnswerCommand;
    }

    public async Task RecordAnswer(RecordAnswerDto recordAnswerDto)
    {
        CreateAnswerCommand createAnswerCommand = new CreateAnswerCommand(_db);
        await createAnswerCommand.CreateAnswer(recordAnswerDto);
    }
}