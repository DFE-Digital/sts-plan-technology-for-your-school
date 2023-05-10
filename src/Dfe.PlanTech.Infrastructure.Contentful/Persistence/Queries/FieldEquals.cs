using Contentful.Core.Search;
using Dfe.PlanTech.Infrastructure.Persistence;
using Dfe.PlanTech.Infrastructure.Persistence.Querying;

namespace Dfe.PlanTech.Infrastructure.Contentful.Persistence.Queries;
public static class FieldEquals
{
    public static QueryBuilder<T> AddToQuery<T>(this ContentQueryEquals query, QueryBuilder<T> queryBuilder)
    => queryBuilder.FieldEquals(query.Field, query.Value);
}
