using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class GiasRepository : IGiasRepository
{
    protected readonly PlanTechDbContext _db;

    public GiasRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public Task<GiasEstablishmentEntity?> GetSingleAcademySchool(int groupUid)
    {
        return _db
            .GiasEstablishmentGroups.Where(e => e.GroupUid == groupUid)
            .Join(
                _db.GiasGroupMemberships,
                establishmentGroup => establishmentGroup.GroupUid,
                groupMembership => groupMembership.GroupUid,
                (establishmentGroup, groupMembership) => groupMembership
            )
            .Join(
                _db.GiasEstablishments,
                groupMembership => groupMembership.Urn,
                establishment => establishment.Urn,
                (groupMembership, establishment) => establishment
            )
            .Include(establishment => establishment.TypeOfEstablishment)
            .SingleOrDefaultAsync();
    }
}
