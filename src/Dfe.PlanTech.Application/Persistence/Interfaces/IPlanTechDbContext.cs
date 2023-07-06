using Dfe.PlanTech.Domain.Answers.Models;
using Dfe.PlanTech.Domain.Users.Models;
using System.Linq.Expressions;

namespace Dfe.PlanTech.Application.Persistence.Interfaces;

public interface IPlanTechDbContext
{
    // User Table & SignIn Table
    IQueryable<User> GetUsers { get; }
    IQueryable<Domain.SignIn.Models.SignIn> SignIns { get; }
    public void AddUser(User user);
    public void AddSignIn(Domain.SignIn.Models.SignIn signIn);

    // Answer Table
    public void AddAnswer(Answer answer);

    // Submission Table
    public void AddSubmission(Domain.Submissions.Models.Submission submission);

    public Task<int> SaveChangesAsync();

    Task<User?> GetUserBy(Expression<Func<User, bool>> predicate);
}

