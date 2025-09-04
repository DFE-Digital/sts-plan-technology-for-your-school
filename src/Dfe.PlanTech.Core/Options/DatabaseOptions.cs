namespace Dfe.PlanTech.Core.Options;

/// <summary>
/// SQL database options; currently only used for retry options.
/// </summary>
public readonly record struct DatabaseOptions
{
    public DatabaseOptions(int maxRetryCount, int maxDelayInMilliseconds)
    {
        MaxRetryCount = maxRetryCount;
        MaxDelayInMilliseconds = maxDelayInMilliseconds;
    }

    public DatabaseOptions() : this(5, 5000) { }

    /// <summary>
    /// Maximum number of retry attempts for transient failures.
    /// </summary>
    public int MaxRetryCount { get; init; }

    /// <summary>
    /// Maximum delay between retry attempts in milliseconds.
    /// </summary>
    public int MaxDelayInMilliseconds { get; init; }
}
