using Dfe.PlanTech.Domain.Answers.Models;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.Questions.Models;
using Dfe.PlanTech.Domain.Responses.Models;
using Dfe.PlanTech.Domain.SignIns.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Models;
using Microsoft.Data.SqlClient;
using System.Linq.Expressions;

namespace Dfe.PlanTech.Application.Persistence.Interfaces;

public interface IPlanTechDbContext
{
    // User Table & SignIn Table
    IQueryable<User> GetUsers { get; }
    IQueryable<SignIn> SignIns { get; }
    public void AddUser(User user);
    public void AddSignIn(SignIn signIn);

    // Question Table
    public void AddQuestion(Question question);
    public Task<Question?> GetQuestion(Expression<Func<Question, bool>> predicate);

    // Answer Table
    public void AddAnswer(Answer answer);
    public Task<Answer?> GetAnswer(Expression<Func<Answer, bool>> predicate);

    // Submission Table
    public IQueryable<Submission> GetSubmissions { get; }
    public void AddSubmission(Submission submission);

    public void AddEstablishment(Establishment establishment);

    // Response Table
    public IQueryable<Response> GetResponses { get; }
    public void AddResponse(Response response);
    public Task<Response[]?> GetResponseList(Expression<Func<Response, bool>> predicate);

    public Task<int> SaveChangesAsync();

    Task<int> CallStoredProcedureWithReturnInt(string sprocName, IEnumerable<SqlParameter> parms, CancellationToken cancellationToken = default);

    IQueryable<SectionStatus> GetSectionStatuses(string sectionIds, int establishmentId);

    Task<User?> GetUserBy(Expression<Func<User, bool>> predicate);

    Task<Establishment?> GetEstablishmentBy(Expression<Func<Establishment, bool>> predicate);

    Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);

    Task<int> ExecuteRaw(FormattableString sql, CancellationToken cancellationToken = default);
}