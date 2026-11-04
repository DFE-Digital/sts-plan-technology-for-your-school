using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class AnswerRepository(PlanTechDbContext dbContext) : IAnswerRepository
{
    protected readonly PlanTechDbContext _db =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<int> GetOrCreateAnswerIdAsync(string answerContentfulId, string answerText)
    {
        var answerId = await _db
            .Answers.Where(a => a.AnswerText == answerText && a.ContentfulRef == answerContentfulId)
            .Select(a => (int?)a.Id)
            .FirstOrDefaultAsync();

        if (answerId.HasValue)
        {
            return answerId.Value;
        }

        var answer = new AnswerEntity
        {
            AnswerText = answerText,
            ContentfulRef = answerContentfulId,
        };

        await _db.Answers.AddAsync(answer);
        await _db.SaveChangesAsync();

        return answer.Id;
    }
}
