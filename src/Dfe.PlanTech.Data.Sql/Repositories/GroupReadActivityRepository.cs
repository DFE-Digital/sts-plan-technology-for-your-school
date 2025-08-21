using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class GroupReadActivityRepository : IGroupReadActivityRepository
{
    protected readonly PlanTechDbContext _db;

    public GroupReadActivityRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public Task<List<GroupReadActivityEntity>> GetGroupReadActivitiesAsync(int userId, int userEstablishmentId)
    {
        return _db.GroupReadActivities
            .Where(x => x.UserId == userId && x.UserEstablishmentId == userEstablishmentId)
            .OrderByDescending(x => x.DateSelected)
            .ToListAsync();
    }
}
