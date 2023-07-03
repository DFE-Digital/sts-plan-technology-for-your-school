using System.Linq.Expressions;
using Dfe.PlanTech.Domain.Answers.Models;

namespace Dfe.PlanTech.Application.Persistence.Interfaces;

public interface IAnswersDbContext
{
    IQueryable<AnswerDto> GetAnswers { get; }

    public void AddAnswer(AnswerDto answer);

    public Task<int> SaveChangesAsync();

    Task<AnswerDto?> GetAnswerBy(Expression<Func<AnswerDto, bool>> predicate);
}

