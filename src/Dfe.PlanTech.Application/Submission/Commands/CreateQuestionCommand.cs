using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Questions.Models;

namespace Dfe.PlanTech.Application.Submission.Commands;

public class CreateQuestionCommand : ICreateQuestionCommand
{
    private readonly IPlanTechDbContext _db;

    public CreateQuestionCommand(IPlanTechDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Creates a new question in the database
    /// </summary>
    /// <param name="recordQuestionDto"></param>
    /// <returns>
    /// The question ID
    /// </returns>
    public async Task<int> CreateQuestion(RecordQuestionDto recordQuestionDto)
    {
        var question = new Question() { QuestionText = recordQuestionDto.QuestionText, ContentfulRef = recordQuestionDto.ContentfulRef };
        _db.AddQuestion(question);
        await _db.SaveChangesAsync();
        return question.Id;
    }
}