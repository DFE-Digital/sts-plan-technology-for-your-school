using Dfe.PlanTech.Domain.DataTransferObjects;
using Dfe.PlanTech.Infrastructure.Data.Entities;

namespace Dfe.PlanTech.Infrastructure.Data.Repositories;

public class SignInRepository
{
    protected readonly PlanTechDbContext _db;

    public SignInRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public async Task<SignInEntity> CreateSignInAsync(int userId, int entityId)
    {
        var signInEntity = new SignInEntity
        {
            EstablishmentId = entityId,
            UserId = userId,
            SignInDateTime = DateTime.UtcNow
        };

        await _db.SignIn.AddAsync(signInEntity);
        await _db.SaveChangesAsync();

        return signInEntity;
    }
}
