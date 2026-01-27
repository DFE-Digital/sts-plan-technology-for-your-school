using System.Linq.Expressions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class EstablishmentLinkRepository : IEstablishmentLinkRepository
{
    protected readonly PlanTechDbContext _db;

    public EstablishmentLinkRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public Task<List<EstablishmentLinkEntity>> GetGroupEstablishmentsByAsync(
        Expression<Func<EstablishmentEntity, bool>> predicate
    )
    {
        return _db
            .Establishments.Where(predicate)
            .Join(
                _db.EstablishmentGroups,
                establishment => establishment.GroupUid,
                group => group.Uid,
                (establishment, group) => group
            )
            .Join(
                _db.EstablishmentLinks,
                group => group.Uid,
                link => link.GroupUid,
                (group, link) => link
            )
            .ToListAsync();
    }

    public Task<List<EstablishmentLinkEntity>> GetGroupEstablishmentsByEstablishmentIdAsync(
        int establishmentId
    )
    {
        return GetGroupEstablishmentsByAsync(establishment => establishment.Id == establishmentId);
    }

    public async Task<int> RecordGroupSelection(UserGroupSelectionModel userGroupSelectionModel)
    {
        ArgumentNullException.ThrowIfNull(userGroupSelectionModel);

        var now = DateTime.UtcNow;

        var entity = new GroupReadActivityEntity
        {
            UserId = userGroupSelectionModel.UserId,
            UserEstablishmentId = userGroupSelectionModel.UserEstablishmentId,
            SelectedEstablishmentId = userGroupSelectionModel.SelectedEstablishmentId,
            SelectedEstablishmentName = userGroupSelectionModel.SelectedEstablishmentName ?? string.Empty,
            DateSelected = now,
        };

        _db.GroupReadActivities.Add(entity);
        await _db.SaveChangesAsync();

        return entity.Id;
    }

}
