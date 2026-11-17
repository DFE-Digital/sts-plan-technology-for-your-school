using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql;

public class TransactionManager(PlanTechDbContext dbContext) : ITransactionManager
{
    protected readonly PlanTechDbContext _db =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(operation);

        // The connection is configured with EnableRetryOnFailure, and a retrying execution
        // strategy refuses a user-initiated transaction unless the transaction is created inside
        // the strategy, so that the whole unit can be retried after a transient failure.
        var executionStrategy = _db.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(async () =>
        {
            // Disposing an uncommitted transaction rolls it back, so the failure path is covered
            // by the await using rather than an explicit catch.
            await using var transaction = await _db.Database.BeginTransactionAsync(
                cancellationToken
            );

            await operation();

            await transaction.CommitAsync(cancellationToken);
        });
    }
}
