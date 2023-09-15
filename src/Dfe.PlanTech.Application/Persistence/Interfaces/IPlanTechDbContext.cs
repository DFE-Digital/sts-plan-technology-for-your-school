using Dfe.PlanTech.Domain.Answers.Models;
using Dfe.PlanTech.Domain.Establishments.Models;
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
    public IQueryable<Domain.Submissions.Models.Submission> GetSubmissions { get; }
    public void AddSubmission(Domain.Submissions.Models.Submission submission);

    public void AddEstablishment(Domain.Establishments.Models.Establishment establishment);

    // Response Table
    public IQueryable<Domain.Responses.Models.Response> GetResponses { get; }
    public void AddResponse(Domain.Responses.Models.Response response);
    public Task<Domain.Responses.Models.Response[]?> GetResponseList(Expression<Func<Domain.Responses.Models.Response, bool>> predicate);

    public Task<int> SaveChangesAsync();

    Task<int> CallStoredProcedureWithReturnInt(string sprocName, IEnumerable<SqlParameter> parms);

    IQueryable<SectionStatuses> GetSectionStatuses(string sectionIds);

    Task<User?> GetUserBy(Expression<Func<User, bool>> predicate);

    Task<Establishment?> GetEstablishmentBy(Expression<Func<Establishment, bool>> predicate);

    Task<List<T>> ToListAsync<T>(IQueryable<T> queryable);

    Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> queryable);
    
    Task<int> ExecuteRaw(FormattableString sql, CancellationToken cancellationToken = default);
}