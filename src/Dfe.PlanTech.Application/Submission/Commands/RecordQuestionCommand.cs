using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Questions.Models;

namespace Dfe.PlanTech.Application.Submission.Commands;

public class RecordQuestionCommand : IRecordQuestionCommand
{
    private readonly IPlanTechDbContext _db;
    private readonly ICreateQuestionCommand _createQuestionCommand;

    public RecordQuestionCommand(IPlanTechDbContext db, ICreateQuestionCommand createQuestionCommand)
    {
        _db = db;
        _createQuestionCommand = createQuestionCommand;
    }

    public async Task<int> RecordQuestion(RecordQuestionDto recordQuestionDto)
    {
        CreateQuestionCommand createQuestionCommand = new CreateQuestionCommand(_db);
        return await createQuestionCommand.CreateQuestion(recordQuestionDto);
    }
}