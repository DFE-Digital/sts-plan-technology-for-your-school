using System.Linq.Expressions;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.SignIns.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Models;
using Microsoft.Data.SqlClient;

namespace Dfe.PlanTech.Application.Persistence.Interfaces;

public interface IPlanTechDbContext
{
    // User Table & SignIn Table
    public void AddUser(User user);
    public void AddSignIn(SignIn signIn);

    // Submission Table
    public IQueryable<Submission> GetSubmissions { get; }

    public IQueryable<ResponseAnswer> GetAnswers { get; }

    public void AddEstablishment(Establishment establishment);

    public Task<int> SaveChangesAsync();

    Task<int> CallStoredProcedureWithReturnInt(string sprocName, IEnumerable<SqlParameter> parms, CancellationToken cancellationToken = default);

    IQueryable<SectionStatusDto> GetSectionStatuses(string categoryId, int establishmentId);

    Task<User?> GetUserBy(Expression<Func<User, bool>> predicate);

    Task<Establishment?> GetEstablishmentBy(Expression<Func<Establishment, bool>> predicate);

    Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);

    Task<int> ExecuteSqlAsync(FormattableString sql, CancellationToken cancellationToken = default);
}
