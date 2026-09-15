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

    private async Task<GroupEstablishmentDTO?> GetGroupAsync(Expression<Func<GiasEstablishmentGroupEntity, bool>> predicate)
    {
        return await _db.GiasEstablishmentGroups
            .Where(predicate)
            .Select(GiasEstablishmentGroupEntity.AsBasicGroupEstablishmentDto)
            .SingleOrDefaultAsync();
    }

    public async Task<List<int>> GetLinkedURNSForGroupAsync(int groupUid)
    {
        return await _db.GiasGroupMemberships
            .Where(x => x.GroupUid == groupUid)
            .Select(x => x.Urn).ToListAsync();
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

    public async Task<GiasEstablishmentEntity?> GetSchoolEstablishmentByURN(int urn)
    {
        return await _db.GiasEstablishments.FindAsync(urn);
    }

    public async Task<bool> AreAllSchoolsWithinGroup(int groupUId, IEnumerable<string> urns)
    {
        var urnInts = urns.Select(int.Parse).Distinct().ToList();

        var matchCount = await _db.GiasGroupMemberships
            .CountAsync(x =>
                x.GroupUid == groupUId &&
                urnInts.Contains(x.Urn));

        return matchCount == urnInts.Count;
    }

    public Task<bool> IsSchoolWithinGroup(int groupUId, string urn)
    {
        var urnInt = int.Parse(urn);

        return _db.GiasGroupMemberships
            .AnyAsync(x =>
                x.GroupUid == groupUId &&
                x.Urn == urnInt);
    }

    public Task<bool> IsSchoolWithinGroup(int groupUId, IEnumerable<string> urns)
    {
        var urnInts = urns.Select(int.Parse).Distinct().ToList();

        return _db.GiasGroupMemberships
            .AnyAsync(x =>
                x.GroupUid == groupUId &&
                urnInts.Contains(x.Urn));
    }
}
