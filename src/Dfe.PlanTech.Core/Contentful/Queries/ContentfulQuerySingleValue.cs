using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Queries;

[ExcludeFromCodeCoverage]
public class ContentfulQuerySingleValue : ContentfulQuery
{
    public string Value { get; init; } = null!;
}
