using System.Linq.Expressions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces
{
    public interface IEstablishmentRepository
    {
        Task<EstablishmentEntity> CreateEstablishmentFromModelAsync(EstablishmentModel model);
        Task<EstablishmentEntity?> GetEstablishmentByReferenceAsync(string establishmentReference);
        Task<List<EstablishmentEntity>> GetEstablishmentsByAsync(Expression<Func<EstablishmentEntity, bool>> predicate);
        Task<List<EstablishmentEntity>> GetEstablishmentsByReferencesAsync(IEnumerable<string> establishmentReferences);
    }
}