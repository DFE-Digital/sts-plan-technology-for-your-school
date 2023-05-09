using Contentful.Core.Search;
using Sts.PlanTech.Infrastructure.Persistence;
using Sts.PlanTech.Infrastructure.Persistence.Querying;

namespace Sts.PlanTech.Infrastructure.Contentful.Persistence.Queries;

public static class FieldIncludes
{
    public static QueryBuilder<T> AddToQuery<T>(this ContentQueryIncludes query, QueryBuilder<T> queryBuilder)
    => queryBuilder.FieldIncludes(query.Field, query.Value);
}