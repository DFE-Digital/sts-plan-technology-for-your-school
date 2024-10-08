namespace Dfe.PlanTech.Application.Persistence.Interfaces;

public interface IDbContext
{
    public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default) where T : class
;
    public Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default) where T : class;
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
