using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Interface;
using Dfe.PlanTech.Domain.Questions.Models;

namespace Dfe.PlanTech.Application.Submission.Queries;

public class GetQuestionQuery : IGetQuestionQuery
{
    private readonly IPlanTechDbContext _db;

    public GetQuestionQuery(IPlanTechDbContext db)
    {
        _db = db;
    }
    public Task<Question?> GetQuestionBy(int questionId)
    {
        return _db.GetQuestionBy(questionId);
    }
}