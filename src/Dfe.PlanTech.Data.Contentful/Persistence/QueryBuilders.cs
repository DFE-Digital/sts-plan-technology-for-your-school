using System.Reflection;
using Contentful.Core.Search;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Data.Contentful.Persistence.Queries;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Queries;

namespace Dfe.PlanTech.Data.Contentful.Persistence;

public static class QueryBuilders
{
    private const string QueryBuilderStringValuesFieldName = "_querystringValues";

    /// <summary>
    /// Builds query builder, filtering by content type
    /// </summary>
    /// <param name="contentTypeId">Content Type Id</param>
    /// <typeparam name="T">Entity type</typeparam>
    /// <returns></returns>
    public static QueryBuilder<T> ByContentType<T>(string contentTypeId)
    {
        if (string.IsNullOrEmpty(contentTypeId))
            throw new ArgumentNullException(nameof(contentTypeId));

        var queryBuilder = new QueryBuilder<T>();
        queryBuilder.ContentTypeIs(contentTypeId);

        return queryBuilder;
    }

    public static QueryBuilder<T> WithQuery<T>(this QueryBuilder<T> queryBuilder, IContentQuery query) =>
    query switch
    {
        ContentQueryEquals equals => equals.AddToQuery(queryBuilder),
        ContentQueryIncludes includes => includes.AddToQuery(queryBuilder),
        _ => throw new ArgumentException($"Could not find correct query builder for ${query.GetType()}")
    };

    public static QueryBuilder<T> WithQueries<T>(this QueryBuilder<T> queryBuilder, IEnumerable<IContentQuery> queries)
    {
        foreach (var query in queries)
        {
            queryBuilder.WithQuery(query);
        }

        return queryBuilder;
    }

    public static QueryBuilder<T> WithQueries<T>(this QueryBuilder<T> queryBuilder, GetEntriesOptions options)
    {
        if (options.Queries != null)
        {
            queryBuilder = queryBuilder.WithQueries(options.Queries);
        }

        return queryBuilder;
    }

    public static QueryBuilder<T> BuildQueryBuilder<T>(string contentTypeId, GetEntriesOptions? options)
    {
        var queryBuilder = ByContentType<T>(contentTypeId);

        if (options != null)
        {
            queryBuilder = queryBuilder.WithOptions(options);
            queryBuilder = queryBuilder.WithSelect(options);
        }

        return queryBuilder;
    }

    public static QueryBuilder<T> WithOptions<T>(this QueryBuilder<T> queryBuilder, GetEntriesOptions options)
    {
        queryBuilder = queryBuilder.WithInclude(options);
        queryBuilder = queryBuilder.WithQueries(options);

        return queryBuilder;
    }

    public static QueryBuilder<T> WithInclude<T>(this QueryBuilder<T> queryBuilder, GetEntriesOptions options)
    {
        queryBuilder.Include(options.Include);
        return queryBuilder;
    }

    public static QueryBuilder<T> WithSelect<T>(this QueryBuilder<T> queryBuilder, GetEntriesOptions options)
    {
        if (options.Select == null)
            return queryBuilder;

        var queryStringValues = queryBuilder.QueryStringValues();

        queryStringValues.Add(new KeyValuePair<string, string>("select", string.Join(',', options.Select)));

        return queryBuilder;
    }

    public static List<KeyValuePair<string, string>> QueryStringValues<T>(this QueryBuilder<T> queryBuilder)
    {
        var fieldInfo = queryBuilder.GetType().GetField(QueryBuilderStringValuesFieldName, BindingFlags.NonPublic | BindingFlags.Instance);

        if (fieldInfo == null)
        {
            throw new MissingFieldException($"Couldn't find field {QueryBuilderStringValuesFieldName}");
        }

        var value = fieldInfo.GetValue(queryBuilder) ?? throw new InvalidDataException($"{QueryBuilderStringValuesFieldName} is null in QueryBuilder");

        if (value is not List<KeyValuePair<string, string>> list)
        {
            throw new InvalidCastException($"Expected {value.GetType()} to be {typeof(List<KeyValuePair<string, string>>)} but is actually {value.GetType()}");
        }

        return list;
    }
}
