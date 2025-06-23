namespace Dfe.PlanTech.Domain.Persistence.Interfaces;

public interface IGetEntriesOptions
{
    /// <summary>
    /// Filter queries (e.g. where field equals value)
    /// </summary>
    public IEnumerable<IContentQuery>? Queries { get; init; }

    /// <summary>
    /// Depth of references to include. 1 = parent only, 2 = parent and child, etc.
    /// </summary>
    public int Include { get; init; }

    /// <summary>
    /// What fields to return from Contentful
    /// </summary>
    /// <remarks>
    /// If null, return all
    /// </remarks>
    public IEnumerable<string>? Select { get; set; }

    public int? Limit { get; init; }
    public int Page { get; init; }

    public string SerializeToRedisFormat();
}
