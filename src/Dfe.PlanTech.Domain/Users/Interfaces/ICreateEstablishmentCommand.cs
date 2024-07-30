using Dfe.PlanTech.Domain.Establishments.Models;

namespace Dfe.PlanTech.Domain.Users.Interfaces;

public interface ICreateEstablishmentCommand
{
    Task<int> CreateEstablishment(EstablishmentDto establishmentDto);
}
