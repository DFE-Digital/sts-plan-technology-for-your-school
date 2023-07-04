using Dfe.PlanTech.Domain.Users.Models;
using System.Linq.Expressions;

namespace Dfe.PlanTech.Application.Persistence.Interfaces;

public interface IUsersDbContext
{
    IQueryable<User> GetUsers { get; }
    IQueryable<Domain.SignIn.Models.SignIn> SignIns { get; }

    public void AddUser(User user);
    public void AddSignIn(Domain.SignIn.Models.SignIn signIn);

    public Task<int> SaveChangesAsync();

    Task<User?> GetUserBy(Expression<Func<User, bool>> predicate);
}

