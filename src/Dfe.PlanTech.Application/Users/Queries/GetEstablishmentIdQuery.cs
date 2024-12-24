using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;

namespace Dfe.PlanTech.Application.Users.Queries;

/// <summary>
/// Query to find a user's Id
/// </summary>
public class GetEstablishmentIdQuery : IGetEstablishmentIdQuery
{
    private readonly IPlanTechDbContext _db;

    public GetEstablishmentIdQuery(IPlanTechDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Finds user matching the given DfeSignInRef and returns their Id field
    /// </summary>
    /// <param name="establishmentRef"></param>
    /// <returns></returns>
    public async Task<int?> GetEstablishmentId(string establishmentRef)
    {
        var establishment = await _db.GetEstablishmentBy(establishment => establishment.EstablishmentRef == establishmentRef);
        return establishment?.Id;
    }

    public async Task<string?> GetEstablishmentGroupName(int establishmentId)
    {
        var establishment = await _db.GetEstablishmentBy(establishment => establishment.Id == establishmentId);
        if (establishment is null)
            return null;

        var group = await _db.GetEstablishmentGroupForEstablishment(establishment);
        return group?.GroupName;
    }
}
