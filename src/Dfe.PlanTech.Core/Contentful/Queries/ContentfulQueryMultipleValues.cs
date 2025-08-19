namespace Dfe.PlanTech.Core.Contentful.Queries;

public class ContentfulQueryMultipleValues : ContentfulQuery
{
    public IEnumerable<string> Value { get; init; } = null!;
}
