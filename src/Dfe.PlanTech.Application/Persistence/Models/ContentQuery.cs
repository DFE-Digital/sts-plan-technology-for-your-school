using Dfe.PlanTech.Domain.Persistence.Interfaces;

namespace Dfe.PlanTech.Infrastructure.Application.Models;

public class ContentQuery : IContentQuery
{
    public string Field { get; init; } = null!;
}
