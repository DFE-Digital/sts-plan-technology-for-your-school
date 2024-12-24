namespace Dfe.PlanTech.Domain.Users.Interfaces;

public interface IGetEstablishmentIdQuery
{
    Task<int?> GetEstablishmentId(string establishmentRef);
    Task<string?> GetEstablishmentGroupName(int establishmentId);
}
