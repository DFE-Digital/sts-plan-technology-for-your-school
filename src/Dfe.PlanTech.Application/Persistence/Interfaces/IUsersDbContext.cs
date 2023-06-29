using System.Linq.Expressions;
using Dfe.PlanTech.Domain.Users.Models;

namespace Dfe.PlanTech.Application.Persistence.Interfaces;

public interface IUsersDbContext
{
    IQueryable<User> GetUsers { get; }

    public void AddUser(User user);

    public Task<int> SaveChangesAsync();

    Task<User?> GetUserBy(Expression<Func<User, bool>> predicate);
}

