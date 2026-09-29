namespace Dfe.PlanTech.Data.Sql.Interfaces;

/// <summary>
/// Runs a sequence of repository operations inside a single database transaction, so that work
/// spanning several repositories either lands in full or not at all.
/// </summary>
public interface ITransactionManager
{
    /// <summary>
    /// Executes <paramref name="operation"/> inside a transaction, committing when it completes
    /// and rolling back if it throws. The operation may be invoked more than once, because it
    /// runs under the connection's retrying execution strategy.
    /// </summary>
    Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default
    );
}
