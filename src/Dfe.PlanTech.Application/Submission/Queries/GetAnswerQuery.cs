using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Interface;
using Dfe.PlanTech.Domain.Answers.Models;

namespace Dfe.PlanTech.Application.Submission.Queries;

public class GetAnswerQuery : IGetAnswerQuery
{
    private readonly IPlanTechDbContext _db;

    public GetAnswerQuery(IPlanTechDbContext db)
    {
        _db = db;
    }
    public Task<Answer?> GetAnswerBy(int answerId)
    {
        return _db.GetAnswerBy(answerId);
    }
}