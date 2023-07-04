using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Answers.Models;

namespace Dfe.PlanTech.Application.Submission.Commands;

public class CreateAnswerCommand : ICreateAnswerCommand
{
    private readonly IAnswersDbContext _db;

    public CreateAnswerCommand(IAnswersDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Creates a new answer in the database
    /// </summary>
    /// <param name="recordAnswerDto"></param>
    /// <returns>
    /// The answer ID
    /// </returns>
    public async Task<int> CreateAnswer(RecordAnswerDto recordAnswerDto)
    {
        _db.AddAnswer(new Answer() { AnswerText = recordAnswerDto.AnswerText, ContentfulRef = recordAnswerDto.ContentfulRef });
        return await _db.SaveChangesAsync();
    }
}