using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class GroupReadActivityRepository(PlanTechDbContext dbContext) : IGroupReadActivityRepository
{
    protected readonly PlanTechDbContext _db =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<int> RecordGroupSelectionAsync(
        UserGroupSelectionModel userGroupSelectionModel
    )
    {
        ArgumentNullException.ThrowIfNull(userGroupSelectionModel);

        var now = DateTime.UtcNow;

        var entity = new GroupReadActivityEntity
        {
            UserId = userGroupSelectionModel.UserId,
            UserEstablishmentId = userGroupSelectionModel.UserEstablishmentId,
            SelectedEstablishmentId = userGroupSelectionModel.SelectedEstablishmentId,
            SelectedEstablishmentName =
                userGroupSelectionModel.SelectedEstablishmentName ?? string.Empty,
            DateSelected = now,
        };

        _db.GroupReadActivities.Add(entity);
        await _db.SaveChangesAsync();

        return entity.Id;
    }
}
