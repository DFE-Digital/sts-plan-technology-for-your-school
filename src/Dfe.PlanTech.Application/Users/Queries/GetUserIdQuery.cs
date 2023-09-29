using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;

namespace Dfe.PlanTech.Application.Users.Queries;

/// <summary>
/// Query to find a user's Id
/// </summary>
public class GetUserIdQuery : IGetUserIdQuery
{
    private readonly IPlanTechDbContext _db;

    public GetUserIdQuery(IPlanTechDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Finds user matching the given DfeSignInRef and returns their Id field
    /// </summary>
    /// <param name="dfeSignInRef"></param>
    /// <returns></returns>
    public async Task<int?> GetUserId(string dfeSignInRef)
    {
        var user = await _db.GetUserBy(user => user.DfeSignInRef == dfeSignInRef);

        return user?.Id;
    }
}
