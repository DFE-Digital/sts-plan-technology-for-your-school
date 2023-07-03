using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Answers.Models;

namespace Dfe.PlanTech.Application.Users.Commands;

public class CreateAnswerCommand : ICreateAnswerCommand
{
    private readonly IAnswersDbContext _db;

    public CreateAnswerCommand(IAnswersDbContext db)
    {
        _db = db;
    }
    public async Task<int> CreateAnswer(RecordAnswerDto recordAnswerDto)
    {
        // TODO: Get AnswerText and ContentfulRef
        AnswerDto answerDto = new AnswerDto() { AnswerText = "Answer", ContentfulRef = "ABC123" };

        _db.AddAnswer(answerDto);

        var answerId = await _db.SaveChangesAsync();

        return answerId;
    }
}