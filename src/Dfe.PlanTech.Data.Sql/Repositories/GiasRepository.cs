using Dfe.PlanTech.Core.DataTransferObjects;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class GiasRepository(PlanTechDbContext dbContext) : IGiasRepository
{
    protected readonly PlanTechDbContext _db =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<GroupEstablishmentDTO?> GetGroupAsync(Expression<Func<GiasEstablishmentGroupEntity, bool>> predicate)
    {
        return await _db.GiasEstablishmentGroups
            .Where(predicate)
            .Select(GiasEstablishmentGroupEntity.AsBasicGroupEstablishmentDto)
            .SingleOrDefaultAsync();
    }

    public Task<GroupEstablishmentDTO?> GetGiasGroupByGroupUIDAsync(int groupUid)
    {
        return GetGroupAsync(group => group.GroupUid == groupUid);
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
