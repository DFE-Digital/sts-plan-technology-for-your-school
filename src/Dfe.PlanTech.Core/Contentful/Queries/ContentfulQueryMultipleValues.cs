using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Queries;

[ExcludeFromCodeCoverage]
public class ContentfulQueryMultipleValues : ContentfulQuery
{
    public IEnumerable<string> Value { get; init; } = null!;
}
