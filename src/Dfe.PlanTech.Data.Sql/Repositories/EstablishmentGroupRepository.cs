using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class EstablishmentGroupRepository(
    PlanTechDbContext dbContext
) : IEstablishmentGroupRepository
{
    private readonly PlanTechDbContext _db =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public Task<List<EstablishmentEntity>> GetLinkedEstablishmentsByGroupEstablishmentIdAsync(
        int establishmentId
    )
    {
        return _db
            .Establishments
            .Where(establishment => establishment.Id == establishmentId)
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
            .Join(
                _db.Establishments,
                link => link.Urn,
                establishment => establishment.EstablishmentRef,
                (link, establishment) => establishment
            )
            .Distinct()
            .ToListAsync();
    }
}
