using System.Linq.Expressions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class EstablishmentRepository(PlanTechDbContext dbContext) : IEstablishmentRepository
{
    protected readonly PlanTechDbContext _db =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<EstablishmentEntity> CreateEstablishmentFromModelAsync(
        EstablishmentModel model
    )
    {
        var establishments = await CreateEstablishmentsFromModelsAsync([model]);
        return establishments.Single();
    }

    public async Task<List<EstablishmentEntity>> CreateEstablishmentsFromModelsAsync(
        IEnumerable<EstablishmentModel> models
    )
    {
        var establishmentEntities = models.Select(model => new EstablishmentEntity
        {
            EstablishmentRef = model.Reference,
            EstablishmentType = model.Type?.Name,
            OrgName = model.Name,
            GroupUid = model.GroupUid, // TODO: Consider getting it from `uid` instead, noting that `GroupUid` is populated from `uid`
        });

        await _db.Establishments.AddRangeAsync(establishmentEntities);
        await _db.SaveChangesAsync();

        return establishmentEntities.ToList();
    }

    public async Task<EstablishmentEntity?> GetEstablishmentByIdAsync(int establishmentId) =>
        await _db.Establishments.FindAsync(establishmentId);

    public Task<List<EstablishmentEntity>> GetEstablishmentsByIdsAsync(
        IEnumerable<int> establishmentIds
    )
    {
        return GetEstablishmentsByAsync(establishment =>
            establishmentIds.Contains(establishment.Id)
        );
    }

    public async Task<EstablishmentEntity?> GetEstablishmentByReferenceAsync(
        string establishmentReference
    )
    {
        var establishments = await GetEstablishmentsByAsync(establishment =>
            establishment.EstablishmentRef == establishmentReference
        );
        return establishments.FirstOrDefault();
    }

    public async Task<List<EstablishmentEntity>> GetEstablishmentsByReferencesAsync(
        IEnumerable<string> establishmentReferences
    )
    {
        var establishments = await GetEstablishmentsByAsync(establishment =>
            establishmentReferences.Contains(establishment.EstablishmentRef)
        );
        return establishments;
    }

    public Task<List<EstablishmentEntity>> GetEstablishmentsByAsync(
        Expression<Func<EstablishmentEntity, bool>> predicate
    )
    {
        return _db
            .Establishments.Where(predicate)
            .Where(establishment => establishment != null)
            .ToListAsync();
    }
}
