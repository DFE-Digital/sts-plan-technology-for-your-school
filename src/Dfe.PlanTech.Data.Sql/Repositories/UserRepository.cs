using System.Linq.Expressions;
using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class UserRepository
{
    protected readonly PlanTechDbContext _db;

    public UserRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public async Task<UserEntity> CreateUserBySignInRefAsync(string dfeSignInReference)
    {
        var userEntity = new UserEntity
        {
            DfeSignInRef = dfeSignInReference,
            DateCreated = DateTime.UtcNow
        };

        await _db.Users.AddAsync(userEntity);
        await _db.SaveChangesAsync();

        return userEntity;
    }

    public async Task<UserEntity?> GetUserBySignInRefAsync(string dfeSignInReference)
    {
        var user = await GetUserByAsync(u => u.DfeSignInRef.Equals(dfeSignInReference));
        return user;
    }

    public Task<UserEntity?> GetUserByAsync(Expression<Func<UserEntity, bool>> predicate)
    {
        return _db.Users.FirstOrDefaultAsync(predicate);
    }
}
