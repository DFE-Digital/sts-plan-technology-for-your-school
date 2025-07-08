using System.Linq.Expressions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql;
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

    public Task<EstablishmentEntity?> GetEstablishmentByRefAsync(string establishmentReference)
    {
        return GetEstablishmentByAsync(establishment => establishment.EstablishmentRef == establishmentReference);
    }

    public Task<EstablishmentEntity?> GetEstablishmentByAsync(Expression<Func<EstablishmentEntity, bool>> predicate)
    {
        return _db.Establishments.FirstOrDefaultAsync(predicate);
    }
}
