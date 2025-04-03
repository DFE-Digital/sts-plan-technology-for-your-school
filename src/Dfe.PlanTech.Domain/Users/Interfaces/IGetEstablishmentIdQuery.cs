using Dfe.PlanTech.Domain.Establishments.Models;

namespace Dfe.PlanTech.Domain.Users.Interfaces;

public interface IGetEstablishmentIdQuery
{
    Task<int?> GetEstablishmentId(string establishmentRef);

    Task<List<EstablishmentLink>> GetGroupEstablishments(int establishmentId);
}
