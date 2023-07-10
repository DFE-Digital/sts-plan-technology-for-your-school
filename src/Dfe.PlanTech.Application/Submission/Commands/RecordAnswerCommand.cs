using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Answers.Models;

namespace Dfe.PlanTech.Application.Submission.Commands;

public class RecordAnswerCommand : IRecordAnswerCommand
{
    private readonly IPlanTechDbContext _db;
    private readonly ICreateAnswerCommand _createAnswerCommand;

    public RecordAnswerCommand(IPlanTechDbContext db, ICreateAnswerCommand createAnswerCommand)
    {
        _db = db;
        _createAnswerCommand = createAnswerCommand;
    }

    public async Task<int> RecordAnswer(RecordAnswerDto recordAnswerDto)
    {
        CreateAnswerCommand createAnswerCommand = new CreateAnswerCommand(_db);
        return await createAnswerCommand.CreateAnswer(recordAnswerDto);
    }
}