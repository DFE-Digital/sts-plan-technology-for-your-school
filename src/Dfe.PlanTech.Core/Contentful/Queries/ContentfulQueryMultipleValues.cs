using Dfe.PlanTech.Core.Content.Queries;

namespace Dfe.PlanTech.Core.Content.ContentfulQueries;

public class ContentfulQueryMultipleValues : ContentfulQuery
{
    public IEnumerable<string> Value { get; init; } = null!;
}
