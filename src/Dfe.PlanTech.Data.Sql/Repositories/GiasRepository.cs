using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class GiasRepository(PlanTechDbContext dbContext) : IGiasRepository
{
    protected readonly PlanTechDbContext _db =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public Task<List<GiasEstablishmentEntity>> GetSchoolsForGroupAsync(int groupUid)
    {
        return _db.GiasEstablishmentGroups
            .AsNoTracking()
            .Where(g => g.GroupUid == groupUid && g.GroupStatusCode == "OPEN")
            .SelectMany(g => g.GroupMemberships)
            .SelectMany(m => m.Establishments)
            .Distinct()
            .ToListAsync();
    }

    public Task<bool> IsSchoolWithinGroupAsync(int groupUid, int schoolUrn)
    {
        return _db.GiasEstablishmentGroups
            .AsNoTracking()
            .Where(g => g.GroupUid == groupUid && g.GroupStatusCode == "OPEN")
            .SelectMany(g => g.GroupMemberships)
            .SelectMany(m => m.Establishments)
            .AnyAsync(e => e.Urn == schoolUrn);
    }

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
