using Contentful.Core.Search;
using Dfe.PlanTech.Infrastructure.Application.Models;

namespace Dfe.PlanTech.Infrastructure.Contentful.Persistence.Queries;
public static class FieldEquals
{
    public static QueryBuilder<T> AddToQuery<T>(this ContentQuerySingleValue query, QueryBuilder<T> queryBuilder)
    => queryBuilder.FieldEquals(query.Field, query.Value);
}
