using Dfe.PlanTech.Domain.Answers.Models;
using Dfe.PlanTech.Domain.Questions.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Models;
using Microsoft.Data.SqlClient;
using System.Linq.Expressions;

namespace Dfe.PlanTech.Application.Persistence.Interfaces;

public interface IPlanTechDbContext
{
    // User Table & SignIn Table
    IQueryable<User> GetUsers { get; }
    IQueryable<Domain.SignIn.Models.SignIn> SignIns { get; }
    public void AddUser(User user);
    public void AddSignIn(Domain.SignIn.Models.SignIn signIn);

    // Question Table
    public void AddQuestion(Question question);
    public Task<Question?> GetQuestion(Expression<Func<Question, bool>> predicate);

    // Answer Table
    public void AddAnswer(Answer answer);
    public Task<Answer?> GetAnswer(Expression<Func<Answer, bool>> predicate);

    // Submission Table
    public void AddSubmission(Domain.Submissions.Models.Submission submission);

    // Response Table
    public void AddResponse(Domain.Responses.Models.Response response);
    public Task<Domain.Responses.Models.Response[]?> GetResponseList(Expression<Func<Domain.Responses.Models.Response, bool>> predicate);

    public Task<int> SaveChangesAsync();

    Task<int> CallStoredProcedureWithReturnInt(string sprocName, List<SqlParameter> parms);

    IQueryable<SectionStatuses> GetSectionStatuses(string sectionIds);

    Task<User?> GetUserBy(Expression<Func<User, bool>> predicate);
}