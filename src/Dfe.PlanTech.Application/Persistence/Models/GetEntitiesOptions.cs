using System.Reflection;
using System.Text;
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

    public string SerializeToRedisKey()
    {
        var builder = new StringBuilder();

        if (Select != null && Select.Any())
        {
            builder.Append(":Select=");
            builder.Append(string.Join(",", Select));
        }

        if (Queries != null && Queries.Any())
        {
            builder.Append(":Queries=[");
            foreach (var query in Queries)
            {
                builder.Append(query.Field);

                var valueProperty = query.GetType().GetProperty("Value");
                var value = valueProperty?.GetValue(query);

                if (value is IEnumerable<string>)
                {
                    builder.Append("=[");
                    builder.Append(string.Join(',', value));
                    builder.Append(']');
                }
                else if (value != null)
                {
                    builder.Append('=');
                    builder.Append(value);
                }
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
