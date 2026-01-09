using System.Reflection;
using Contentful.Core.Search;
using Dfe.PlanTech.Core.Contentful.Options;

namespace Dfe.PlanTech.Core.Contentful.Queries;

public static class QueryBuilders
{
    private const string QueryBuilderStringValuesFieldName = "_querystringValues";

    /// <summary>
    /// Builds query builder, filtering by content type
    /// </summary>
    /// <param name="contentfulContentTypeId">Content Type Id</param>
    /// <typeparam name="T">Entity type</typeparam>
    /// <returns></returns>
    public static QueryBuilder<T> ByContentType<T>(string? contentfulContentTypeId)
    {
        if (string.IsNullOrEmpty(contentfulContentTypeId))
            throw new ArgumentNullException(nameof(contentfulContentTypeId));

        var queryBuilder = new QueryBuilder<T>();
        queryBuilder.ContentTypeIs(contentfulContentTypeId);

        return queryBuilder;
    }

    public static QueryBuilder<T> WithQuery<T>(
        this QueryBuilder<T> queryBuilder,
        ContentfulQuery query
    ) =>
        query switch
        {
            ContentfulQuerySingleValue equals => equals.AddToQuery(queryBuilder),
            ContentfulQueryMultipleValues includes => includes.AddToQuery(queryBuilder),
            _ => throw new ArgumentException(
                $"Could not find correct query builder for ${query.GetType()}"
            ),
        };

    public static QueryBuilder<T> WithQueries<T>(
        this QueryBuilder<T> queryBuilder,
        IEnumerable<ContentfulQuery> queries
    )
    {
        foreach (var query in queries)
        {
            queryBuilder.WithQuery(query);
        }

        return queryBuilder;
    }

    public static QueryBuilder<T> WithQueries<T>(
        this QueryBuilder<T> queryBuilder,
        GetEntriesOptions options
    )
    {
        if (options.Queries != null)
        {
            queryBuilder = queryBuilder.WithQueries(options.Queries);
        }

        return queryBuilder;
    }

    public static QueryBuilder<T> BuildQueryBuilder<T>(
        string contentfulContentTypeId,
        GetEntriesOptions? options
    )
    {
        var queryBuilder = ByContentType<T>(contentfulContentTypeId);

        if (options != null)
        {
            queryBuilder = queryBuilder.WithOptions(options);
            queryBuilder = queryBuilder.WithSelect(options);
        }

        return queryBuilder;
    }

    public static QueryBuilder<T> WithOptions<T>(
        this QueryBuilder<T> queryBuilder,
        GetEntriesOptions options
    )
    {
        queryBuilder = queryBuilder.WithInclude(options);
        queryBuilder = queryBuilder.WithQueries(options);

        return queryBuilder;
    }

    public static QueryBuilder<T> WithInclude<T>(
        this QueryBuilder<T> queryBuilder,
        GetEntriesOptions options
    )
    {
        queryBuilder.Include(options.Include);
        return queryBuilder;
    }

    public static QueryBuilder<T> WithSelect<T>(
        this QueryBuilder<T> queryBuilder,
        GetEntriesOptions options
    )
    {
        if (options.Select == null)
            return queryBuilder;

        var queryStringValues = queryBuilder.QueryStringValues();

        queryStringValues.Add(
            new KeyValuePair<string, string>("select", string.Join(',', options.Select))
        );

        return queryBuilder;
    }

    public static List<KeyValuePair<string, string>> QueryStringValues<T>(
        this QueryBuilder<T> queryBuilder
    )
    {
        // Intentional use of reflection to access internal QueryBuilder state.
        // Field name is constant and type is trusted.
        var fieldInfo = queryBuilder
            .GetType()
            .GetField(
                QueryBuilderStringValuesFieldName,
                BindingFlags.NonPublic | BindingFlags.Instance
            );

        if (fieldInfo == null)
        {
            throw new MissingFieldException(
                $"Couldn't find field {QueryBuilderStringValuesFieldName}"
            );
        }

        var value =
            fieldInfo.GetValue(queryBuilder)
            ?? throw new InvalidDataException(
                $"{QueryBuilderStringValuesFieldName} is null in QueryBuilder"
            );

        if (value is not List<KeyValuePair<string, string>> list)
        {
            throw new InvalidCastException(
                $"Expected {value.GetType()} to be {typeof(List<KeyValuePair<string, string>>)} but is actually {value.GetType()}"
            );
        }

        return list;
    }
}
