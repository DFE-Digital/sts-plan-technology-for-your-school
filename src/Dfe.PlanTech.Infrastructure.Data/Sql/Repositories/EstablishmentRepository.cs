using System.Linq.Expressions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Infrastructure.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Repositories;

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

    public Task<EstablishmentEntity?> GetEstablishmentByRefAsync(string establishmentRef)
    {
        return GetEstablishmentByAsync(establishment => establishment.EstablishmentRef == establishmentRef);
    }

    public Task<EstablishmentEntity?> GetEstablishmentByAsync(Expression<Func<EstablishmentEntity, bool>> predicate)
    {
        return _db.Establishments.FirstOrDefaultAsync(predicate);
    }
}
