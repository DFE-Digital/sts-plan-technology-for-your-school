namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Queries;

public class ContentfulQueryMultipleValues : ContentfulQuery
{
    public IEnumerable<string> Value { get; init; } = null!;
}
