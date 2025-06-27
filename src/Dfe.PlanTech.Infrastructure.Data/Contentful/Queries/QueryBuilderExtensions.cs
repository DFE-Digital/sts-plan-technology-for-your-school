using Contentful.Core.Search;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Queries
{
    public static class QueryBuilderExtensions
    {
        public static QueryBuilder<T> AddToQuery<T>(this ContentfulQuerySingleValue query, QueryBuilder<T> queryBuilder)
            => queryBuilder.FieldEquals(query.Field, query.Value);

        public static QueryBuilder<T> AddToQuery<T>(this ContentfulQueryMultipleValues query, QueryBuilder<T> queryBuilder)
            => queryBuilder.FieldIncludes(query.Field, query.Value);
    }
}
