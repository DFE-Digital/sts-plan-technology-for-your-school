using Contentful.Core.Search;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Persistence.Queries;

namespace Dfe.PlanTech.Infrastructure.Contentful.Persistence;

public static class QueryBuilders
{
    /// <summary>
    /// Builds query builder, filtering by content type
    /// </summary>
    /// <param name="contentTypeId">Content Type Id</param>
    /// <typeparam name="T">Entity type</typeparam>
    /// <returns></returns>
    public static QueryBuilder<T> ByContentType<T>(string contentTypeId)
    {
        if (string.IsNullOrEmpty(contentTypeId)) throw new ArgumentNullException(nameof(contentTypeId));

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


    public static QueryBuilder<T> WithQueries<T>(this QueryBuilder<T> queryBuilder, IGetEntitiesOptions options)
    {
        if (options.Queries != null)
        {
            queryBuilder = queryBuilder.WithQueries(options.Queries);
        }

        return queryBuilder;
    }


    public static QueryBuilder<T> BuildQueryBuilder<T>(string contentTypeId, IGetEntitiesOptions options)
    {
        var queryBuilder = ByContentType<T>(contentTypeId);
        queryBuilder = queryBuilder.WithInclude(options);
        queryBuilder = queryBuilder.WithQueries(options);

        return queryBuilder;
    }

    public static QueryBuilder<T> WithInclude<T>(this QueryBuilder<T> queryBuilder, IGetEntitiesOptions options)
    {
        queryBuilder.Include(options.Include);
        return queryBuilder;
    }
}
