using System.Text;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Queries;

public class GetEntriesOptions
{
    public GetEntriesOptions(int include, IEnumerable<ContentfulQuery> queries)
        : this(include)
    {
        Include = include;
        Queries = queries;
    }

    public GetEntriesOptions(int include)
    {
        Include = include;
    }

    public GetEntriesOptions(IEnumerable<ContentfulQuery> queries)
    {
        Queries = queries;
    }

    public GetEntriesOptions()
    {
    }

    public int Page { get; init; } = 1;

    public int? Limit { get; init; }

    public IEnumerable<string>? Select { get; set; }

    public IEnumerable<ContentfulQuery>? Queries { get; init; }

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

                if (query is ContentfulQuerySingleValue queryEquals)
                    builder.Append($"={queryEquals.Value}");
                else if (query is ContentfulQueryMultipleValues queryIncludes)
                    builder.Append($"=[{string.Join(',', queryIncludes.Value)}]");

                builder.Append(',');
            }
            builder.Length--;
            builder.Append(']');
        }

        return builder.ToString();
    }
}
