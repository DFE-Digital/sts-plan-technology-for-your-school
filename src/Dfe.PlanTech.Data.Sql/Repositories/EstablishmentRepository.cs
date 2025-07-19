using System.Linq.Expressions;
using Dfe.PlanTech.Core.RoutingDataModel;
using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class EstablishmentRepository
{
    protected readonly PlanTechDbContext _db;

    public EstablishmentRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public async Task<EstablishmentEntity> CreateEstablishmentFromModelAsync(EstablishmentModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var establishmentEntity = new EstablishmentEntity()
        {
            EstablishmentRef = model.Reference,
            EstablishmentType = model.Type?.Name,
            OrgName = model.OrgName,
            GroupUid = model.GroupUid
        };

        await _db.Establishments.AddAsync(establishmentEntity);
        await _db.SaveChangesAsync();

        return establishmentEntity;
    }

    public Task<List<EstablishmentEntity>> GetEstablishmentsByReferencesAsync(IEnumerable<string> establishmentReferences)
    {
        return GetEstablishmentsByAsync(establishment => establishmentReferences.Contains(establishment.EstablishmentRef));
    }

    public Task<List<EstablishmentEntity>> GetEstablishmentsByAsync(Expression<Func<EstablishmentEntity, bool>> predicate)
    {
        return _db.Establishments
            .Where(predicate)
            .Where(establishment => establishment != null)
            .ToListAsync();
    }
}
