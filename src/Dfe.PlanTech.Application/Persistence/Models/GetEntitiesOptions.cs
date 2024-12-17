using System.Text;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Infrastructure.Application.Models;

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

    public string SerializeToRedisFormat()
    {
        var builder = new StringBuilder();

        if (Select != null && Select.Any())
            builder.Append($":Select=[{string.Join(",", Select)}]");

        if (Queries != null && Queries.Any())
        {
            builder.Append(":Queries=[");
            foreach (var query in Queries)
            {
                builder.Append(query.Field);

                if (query is ContentQueryEquals queryEquals)
                    builder.Append($"={queryEquals.Value}");
                else if (query is ContentQueryIncludes queryIncludes)
                    builder.Append($"=[{string.Join(',', queryIncludes.Value)}]");

                builder.Append(',');
            }
            builder.Length--;
            builder.Append(']');
        }

        builder.Append(":Include=");
        builder.Append(Include);

        return builder.ToString();
    }
}
