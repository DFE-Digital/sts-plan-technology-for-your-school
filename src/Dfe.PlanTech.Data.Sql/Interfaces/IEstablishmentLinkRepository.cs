using System.Linq.Expressions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IEstablishmentLinkRepository
{
    Task<int> RecordGroupSelection(UserGroupSelectionModel userGroupSelectionModel);
    Task<List<EstablishmentLinkEntity>> GetGroupEstablishmentsByAsync(Expression<Func<EstablishmentEntity, bool>> predicate);
    Task<List<EstablishmentLinkEntity>> GetGroupEstablishmentsByEstablishmentIdAsync(int establishmentId);
}
