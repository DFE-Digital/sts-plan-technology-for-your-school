using Dfe.PlanTech.Domain.DataTransferObjects;
using Dfe.PlanTech.Infrastructure.Data.Sql.Entities;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Repositories;

public class SignInRepository
{
    protected readonly PlanTechDbContext _db;

    public SignInRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public async Task<SignInEntity> CreateSignInAsync(int establishmentId, int userId)
    {
        var signInEntity = new SignInEntity
        {
            EstablishmentId = establishmentId,
            UserId = userId,
            SignInDateTime = DateTime.UtcNow
        };

        await _db.SignIn.AddAsync(signInEntity);
        await _db.SaveChangesAsync();

        return signInEntity;
    }
}
