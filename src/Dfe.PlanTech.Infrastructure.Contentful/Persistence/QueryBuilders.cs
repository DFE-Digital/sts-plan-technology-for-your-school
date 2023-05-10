using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contentful.Core.Search;
using Dfe.PlanTech.Infrastructure.Contentful.Persistence.Queries;
using Dfe.PlanTech.Infrastructure.Persistence;
using Dfe.PlanTech.Infrastructure.Persistence.Querying;

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

    public static QueryBuilder<T> WithQuery<T>(this QueryBuilder<T> queryBuilder, ContentQuery query) =>
    query switch
    {
        ContentQueryEquals equals => equals.AddToQuery(queryBuilder),
        ContentQueryIncludes includes => includes.AddToQuery(queryBuilder),
        _ => throw new ArgumentException($"Could not find correct query builder for ${query.GetType()}")
    };

    public static QueryBuilder<T> WithQueries<T>(this QueryBuilder<T> queryBuilder, IEnumerable<ContentQuery> queries)
    {
        foreach (var query in queries)
        {
            queryBuilder.WithQuery(query);
        }
        
        return queryBuilder;
    }
}
