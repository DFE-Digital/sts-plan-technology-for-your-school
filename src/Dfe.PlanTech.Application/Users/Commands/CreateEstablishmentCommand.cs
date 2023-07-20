using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Establishments.Models;

namespace Dfe.PlanTech.Application.Users.Commands;

public class CreateEstablishmentCommand : ICreateEstablishmentCommand
{
    private readonly IPlanTechDbContext _db;

    public CreateEstablishmentCommand(IPlanTechDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Creates new user and returns ID
    /// </summary>
    /// <param name="establishmentDto"></param>
    /// <returns></returns>
    public async Task<int> CreateEstablishment(EstablishmentDto establishmentDto)
    {
        var establishment = new Establishment()
        {
            EstablishmentRef = establishmentDto.EstablishmentRef,
            EstablishmentType = establishmentDto.EstablishmentType,
            OrgName = establishmentDto.OrgName,
        };

        _db.AddEstablishment(establishment);

        var establishmentId = await _db.SaveChangesAsync();

        return establishmentId;
    }
}