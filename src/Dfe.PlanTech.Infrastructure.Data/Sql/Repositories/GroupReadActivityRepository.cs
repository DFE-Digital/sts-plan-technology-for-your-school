using Dfe.PlanTech.Infrastructure.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Repositories;

public class GroupReadActivityRepository
{
    protected readonly PlanTechDbContext _db;

    public GroupReadActivityRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public Task<List<GroupReadActivityEntity>> GetGroupReadActivitiesAsync()
    {
        return _db.Set<GroupReadActivityEntity>().ToListAsync();
    }
}
