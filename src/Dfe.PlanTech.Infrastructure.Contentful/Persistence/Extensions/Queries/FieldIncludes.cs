using Contentful.Core.Search;
using Dfe.PlanTech.Infrastructure.Application.Models;

namespace Dfe.PlanTech.Infrastructure.Contentful.Persistence.Queries;

public static class FieldIncludes
{
    public static QueryBuilder<T> AddToQuery<T>(this ContentQueryIncludes query, QueryBuilder<T> queryBuilder)
    => queryBuilder.FieldIncludes(query.Field, query.Value);
}
