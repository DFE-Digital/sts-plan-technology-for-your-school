namespace Dfe.PlanTech.Application.Persistence.Interfaces;

public interface IPlanTechDbContext
{
    // User Table & SignIn Table
    public void AddUser(User user);
    public void AddSignIn(SignIn signIn);

    // Submission Table
    public IQueryable<Submission> GetSubmissions { get; }

    public void AddEstablishment(Establishment establishment);

    public Task<int> SaveChangesAsync();

    Task<int> CallStoredProcedureWithReturnInt(string sprocName, IEnumerable<SqlParameter> parms, CancellationToken cancellationToken = default);

    IQueryable<SectionStatusDto> GetSectionStatuses(string sectionIds, int establishmentId);

    Task<User?> GetUserBy(Expression<Func<User, bool>> predicate);

    Task<Establishment?> GetEstablishmentBy(Expression<Func<Establishment, bool>> predicate);

    Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);

    Task<int> ExecuteRaw(FormattableString sql, CancellationToken cancellationToken = default);
}