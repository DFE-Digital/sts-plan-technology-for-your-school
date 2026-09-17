using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class QuestionRepository(PlanTechDbContext dbContext) : IQuestionRepository
{
    protected readonly PlanTechDbContext _db =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<List<QuestionEntity>> GetQuestionsForSection(
        QuestionnaireSectionEntry section
    )
    {
        var sectionQuestionRefs = section.Questions.Select(q => q.Sys?.Id).ToList();

        var sectionQuestions = await _db
            .Questions.Where(question => sectionQuestionRefs.Contains(question.ContentfulRef))
            .ToListAsync();

        return sectionQuestions;
    }

    public async Task<int> GetOrCreateQuestionIdAsync(
        string questionContentfulId,
        string questionText
    )
    {
        var questionId = await _db
            .Questions.Where(q =>
                q.QuestionText == questionText && q.ContentfulRef == questionContentfulId
            )
            .Select(q => (int?)q.Id)
            .FirstOrDefaultAsync();

        if (questionId.HasValue)
        {
            return questionId.Value;
        }

        var question = new QuestionEntity
        {
            QuestionText = questionText,
            ContentfulRef = questionContentfulId,
        };

        await _db.Questions.AddAsync(question);
        await _db.SaveChangesAsync();

        return question.Id;
    }
}
