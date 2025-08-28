using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

[ExcludeFromCodeCoverage]
public class EstablishmentRepository : IEstablishmentRepository
{
    protected readonly PlanTechDbContext _db;

    public EstablishmentRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public async Task<EstablishmentEntity> CreateEstablishmentFromModelAsync(EstablishmentModel model)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        var establishmentEntity = new EstablishmentEntity()
        {
            EstablishmentRef = model.Reference,
            EstablishmentType = model.Type?.Name,
            OrgName = model.Name,
            GroupUid = model.GroupUid
        };

        await _db.Establishments.AddAsync(establishmentEntity);
        await _db.SaveChangesAsync();

        return establishmentEntity;
    }

    public async Task<EstablishmentEntity?> GetEstablishmentByReferenceAsync(string establishmentReference)
    {
        var establishments = await GetEstablishmentsByAsync(establishment => establishment.EstablishmentRef.Equals(establishmentReference));
        return establishments.FirstOrDefault();
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
