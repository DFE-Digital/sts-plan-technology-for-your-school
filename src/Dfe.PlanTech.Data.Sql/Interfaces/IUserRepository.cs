using System.Linq.Expressions;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IUserRepository
{
    Task<UserEntity> CreateUserBySignInRefAsync(string dfeSignInReference);
    Task<UserEntity?> GetUserByAsync(Expression<Func<UserEntity, bool>> predicate);
    Task<UserEntity?> GetUserBySignInRefAsync(string dfeSignInReference);
}
