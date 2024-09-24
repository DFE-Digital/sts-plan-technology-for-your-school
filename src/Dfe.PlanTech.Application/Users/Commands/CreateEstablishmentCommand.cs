using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;

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
        ArgumentNullException.ThrowIfNull(establishmentDto);

        var establishment = new Establishment()
        {
            EstablishmentRef = establishmentDto.Reference,
            EstablishmentType = establishmentDto.Type?.Name,
            OrgName = establishmentDto.OrgName,
        };

        _db.AddEstablishment(establishment);

        await _db.SaveChangesAsync();

        return establishment.Id;
    }
}
