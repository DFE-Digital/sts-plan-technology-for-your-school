using Dfe.PlanTech.Domain.Questions.Models;
using Dfe.PlanTech.Domain.Answers.Models;
using Dfe.PlanTech.Domain.Users.Models;
using System.Linq.Expressions;
using Dfe.PlanTech.Domain.Responses.Models;
using Microsoft.Data.SqlClient;

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

    public Task<Question?> GetQuestionBy(int questionId);

    // Answer Table
    public void AddAnswer(Answer answer);

    // Submission Table
    public void AddSubmission(Domain.Submissions.Models.Submission submission);

    // Response Table
    public void AddResponse(Domain.Responses.Models.Response response);

    public Task<int> SaveChangesAsync();

    Task<int> CallStoredProcedureWithReturnInt(string sprocName, List<SqlParameter> parms);

    Task<User?> GetUserBy(Expression<Func<User, bool>> predicate);

    Task<Domain.Responses.Models.Response[]?> GetResponseListBy(int submissionId);
}

