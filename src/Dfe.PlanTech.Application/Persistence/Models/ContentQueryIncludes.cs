namespace Dfe.PlanTech.Infrastructure.Application.Models;

public class ContentQueryIncludes : ContentQuery
{
    public IEnumerable<string> Value { get; init; } = null!;
}
