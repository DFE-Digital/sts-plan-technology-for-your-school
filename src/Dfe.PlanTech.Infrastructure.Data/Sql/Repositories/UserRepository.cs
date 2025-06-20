using System.Linq.Expressions;
using Dfe.PlanTech.Infrastructure.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Repositories;

public class UserRepository
{
    protected readonly PlanTechDbContext _db;

    public UserRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public async Task<UserEntity> CreateUserBySignInRefAsync(string dfeSignInRef)

    {
        var userEntity = new UserEntity
        {
            DfeSignInRef = dfeSignInRef,
            DateCreated = DateTime.UtcNow
        };

        await _db.Users.AddAsync(userEntity);
        await _db.SaveChangesAsync();

        return userEntity;
    }

    public async Task<UserEntity?> GetUserBySignInRefAsync(string dfeSignInRef)
    {
        var user = await GetUserByAsync(user => user.DfeSignInRef == dfeSignInRef);
        return user;
    }

    public Task<UserEntity?> GetUserByAsync(Expression<Func<UserEntity, bool>> predicate)
    {
        return _db.Users.FirstOrDefaultAsync(predicate);
    }
}
