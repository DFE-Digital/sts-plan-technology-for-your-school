using Dfe.PlanTech.Domain.Establishments.Models;

namespace Dfe.PlanTech.Application.Persistence.Interfaces;

public interface ICreateEstablishmentCommand
{
    Task<int> CreateEstablishment(EstablishmentDto establishmentDto);
}