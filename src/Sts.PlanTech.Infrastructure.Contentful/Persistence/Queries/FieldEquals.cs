using Contentful.Core.Search;
using Sts.PlanTech.Infrastructure.Persistence;
using Sts.PlanTech.Infrastructure.Persistence.Querying;

namespace Sts.PlanTech.Infrastructure.Contentful.Persistence.Queries;
public static class FieldEquals
{
    public static QueryBuilder<T> AddToQuery<T>(this ContentQueryEquals query, QueryBuilder<T> queryBuilder)
    => queryBuilder.FieldEquals(query.Field, query.Value);
}
