using Dfe.PlanTech.Domain.Answers.Models;

namespace Dfe.PlanTech.Application.Persistence.Interfaces;

public interface IAnswersDbContext
{
    public void AddAnswer(Answer answer);

    public Task<int> SaveChangesAsync();

}

