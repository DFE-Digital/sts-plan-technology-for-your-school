using Dfe.PlanTech.Application.Persistence.Interfaces;

namespace Dfe.PlanTech.Infrastructure.Application.Models;

public class ContentQuery : IContentQuery
{
    public string Field { get; init; } = null!;
}

public class ContentQueryEquals : ContentQuery
{
    public string Value { get; init; } = null!;
}

public class ContentQueryIncludes : ContentQuery
{
    public IEnumerable<string> Value { get; init; } = null!;
}
