using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Response.Interface;

namespace Dfe.PlanTech.Application.Response.Queries;

public class GetResponseQuery : IGetResponseQuery
{

    private readonly IPlanTechDbContext _db;

    public GetResponseQuery(IPlanTechDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Finds a list of responses matching the submissionId and returns them
    /// </summary>
    /// <param name="submissionId"></param>
    /// <returns></returns>
    public Task<Domain.Responses.Models.Response[]?> GetResponseListBy(int submissionId)
    {
        return _db.GetResponseList(response => response.SubmissionId == submissionId);
    }
}