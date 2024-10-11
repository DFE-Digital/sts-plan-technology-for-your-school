namespace Dfe.PlanTech.Application.Persistence.Interfaces;

public interface IDbContext
{
    public Task<T?> FirstOrDefaultCachedAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);
    public Task<List<T>> ToListCachedAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);
    public Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);
    public Task<T> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
