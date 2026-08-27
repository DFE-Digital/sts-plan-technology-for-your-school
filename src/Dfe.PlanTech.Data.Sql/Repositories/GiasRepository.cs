using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class GiasRepository(PlanTechDbContext dbContext) : IGiasRepository
{
    protected readonly PlanTechDbContext _db =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public Task<GiasEstablishmentEntity?> GetSingleAcademySchool(int groupUid)
    {
        return _db
            .GiasEstablishmentGroups.Where(eg =>
                eg.GroupUid == groupUid && eg.GroupStatusCode.Equals("OPEN")
            )
            .SelectMany(eg => eg.GroupMemberships)
            .SelectMany(gm => gm.Establishments)
            .Include(e => e.TypeOfEstablishment)
            .SingleOrDefaultAsync();
    }
}
