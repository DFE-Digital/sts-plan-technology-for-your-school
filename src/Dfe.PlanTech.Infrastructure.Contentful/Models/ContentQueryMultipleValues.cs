namespace Dfe.PlanTech.Infrastructure.Application.Models;

public class ContentQueryMultipleValues : ContentQuery
{
    public IEnumerable<string> Value { get; init; } = null!;
}
