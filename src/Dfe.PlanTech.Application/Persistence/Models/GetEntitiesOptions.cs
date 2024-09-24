using Dfe.PlanTech.Domain.Persistence.Interfaces;

namespace Dfe.PlanTech.Application.Persistence.Models;

public class GetEntitiesOptions : IGetEntitiesOptions
{
    public GetEntitiesOptions(int include)
    {
        Include = include;
    }

    public GetEntitiesOptions(int include, IEnumerable<IContentQuery> queries)
    {
        Include = include;
        Queries = queries;
    }

    public GetEntitiesOptions(IEnumerable<IContentQuery> queries)
    {
        Queries = queries;
    }

    public GetEntitiesOptions()
    {
    }

    public IEnumerable<string>? Select { get; set; }

    public IEnumerable<IContentQuery>? Queries { get; init; }

    public int Include { get; init; } = 2;
}
