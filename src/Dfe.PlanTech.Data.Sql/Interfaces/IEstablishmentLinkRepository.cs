using System.Linq.Expressions;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IEstablishmentLinkRepository
{
    Task<List<EstablishmentLinkEntity>> GetGroupEstablishmentsByAsync(Expression<Func<EstablishmentEntity, bool>> predicate);
    Task<List<EstablishmentLinkEntity>> GetGroupEstablishmentsByEstablishmentIdAsync(int establishmentId);
}
