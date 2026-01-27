using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class SignInRepository : ISignInRepository
{
    protected readonly PlanTechDbContext _db;

    public SignInRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public async Task<SignInEntity> CreateSignInAsync(int userId, int? establishmentId = null)
    {
        if (userId.Equals(0))
        {
            throw new ArgumentException($"{nameof(userId)} cannot be 0.");
        }

        var signInEntity = new SignInEntity
        {
            SignInDateTime = DateTime.UtcNow,
            UserId = userId,
            EstablishmentId = establishmentId,
        };

        await _db.SignIns.AddAsync(signInEntity);
        await _db.SaveChangesAsync();

        return signInEntity;
    }

    public async Task<SignInEntity> RecordSignInWithoutEstablishmentIdAsync(string dfeSignInRef)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.DfeSignInRef.Equals(dfeSignInRef));
        if (user is null)
        {
            throw new ArgumentException($"Could not find user with reference '{dfeSignInRef}'");
        }

        var signInEntity = new SignInEntity { SignInDateTime = DateTime.UtcNow, UserId = user.Id };

        await _db.SignIns.AddAsync(signInEntity);
        await _db.SaveChangesAsync();

        return signInEntity;
    }
}
