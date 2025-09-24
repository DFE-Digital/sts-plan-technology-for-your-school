namespace Dfe.PlanTech.Core.Contentful.Models.Options;

public class GetPageFromContentfulOptions
{
    /// <summary>
    /// How many reference levels to include in the query
    /// </summary>
    /// <remarks>
    /// E.g. 1 == just the parent, 2 == parent and child, etc.
    /// </remarks>
    public int Include { get; init; } = 4;
}
