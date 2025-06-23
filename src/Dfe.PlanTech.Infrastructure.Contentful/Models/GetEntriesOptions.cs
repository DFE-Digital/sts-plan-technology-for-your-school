using System.Text;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Infrastructure.Application.Models;

namespace Dfe.PlanTech.Application.Persistence.Models;

public class GetEntriesOptions : IGetEntriesOptions
{
    public GetEntriesOptions(int include, IEnumerable<IContentQuery> queries)
        : this(include)
    {
        Include = include;
        Queries = queries;
    }

    public GetEntriesOptions(int include)
    {
        Include = include;
    }

    public GetEntriesOptions(IEnumerable<IContentQuery> queries)
    {
        Queries = queries;
    }

    public GetEntriesOptions()
    {
    }

    public int Page { get; init; } = 1;

    public int? Limit { get; init; }

    public IEnumerable<string>? Select { get; set; }

    public IEnumerable<IContentQuery>? Queries { get; init; }

    public int Include { get; init; } = 2;

    public string SerializeToRedisFormat()
    {
        var builder = new StringBuilder();

        builder.Append(":Include=");
        builder.Append(Include);

        if (Select != null && Select.Any())
            builder.Append($":Select=[{string.Join(",", Select)}]");

        if (Queries != null && Queries.Any())
        {
            builder.Append(":Queries=[");
            foreach (var query in Queries)
            {
                builder.Append(query.Field);

                if (query is ContentQuerySingleValue queryEquals)
                    builder.Append($"={queryEquals.Value}");
                else if (query is ContentQueryMultipleValues queryIncludes)
                    builder.Append($"=[{string.Join(',', queryIncludes.Value)}]");

                builder.Append(',');
            }
            builder.Length--;
            builder.Append(']');
        }

        return builder.ToString();
    }
}
