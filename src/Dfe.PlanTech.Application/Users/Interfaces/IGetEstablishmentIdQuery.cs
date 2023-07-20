namespace Dfe.PlanTech.Application.Users.Interfaces;

public interface IGetEstablishmentIdQuery
{
    Task<int?> GetEstablishmentId(string establishmentRef);
}