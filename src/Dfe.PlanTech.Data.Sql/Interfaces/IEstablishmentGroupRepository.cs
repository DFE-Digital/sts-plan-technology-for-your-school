using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IEstablishmentGroupRepository
{
    Task<List<EstablishmentEntity>> GetLinkedEstablishmentsByGroupEstablishmentIdAsync(
        int establishmentId
    );
}
