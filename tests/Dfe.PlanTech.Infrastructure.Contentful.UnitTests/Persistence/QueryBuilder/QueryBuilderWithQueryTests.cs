using Contentful.Core.Search;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Data.Contentful.Persistence;

namespace Dfe.PlanTech.Data.Contentful.UnitTests;

public class QueryBuilderWithQueryTests
{
    private const string TEST_VALUE = "TestValue";
    private const string TEST_FIELD = "TestField";

    private readonly ContentQueryMultipleValues INCLUDES = new()
    {
        Value = new[] { TEST_VALUE, "OtherTestValue" },
        Field = TEST_FIELD
    };

    private readonly ContentQuerySingleValue EQUALS = new()
    {
        Value = TEST_VALUE,
        Field = TEST_FIELD
    };

    [Fact]
    public void Should_Call_ContentQueryEquals_When_ClassIs_ContentQueryEquals()
    {
        var query = EQUALS;

        var queryBuilder = new QueryBuilder<TestClass>();

        queryBuilder.WithQuery(query);

        var queryString = queryBuilder.Build();

        string expected = GetExpectedStringForEquals(query);
        Assert.Contains(expected, queryString);
    }

    [Fact]
    public void Should_Call_ContentQueryIncludes_When_ClassIs_ContentQueryIncludes()
    {
        var query = INCLUDES;
        var queryBuilder = new QueryBuilder<TestClass>();

        queryBuilder.WithQuery(query);

        var queryString = queryBuilder.Build();

        string expected = GetExpectedStringForIncludes(query);
        Assert.Contains(expected, queryString);
    }

    [Fact]
    public void Should_Throw_When_Not_Concrete_Type()
    {
        var query = new ContentQuery()
        {
            Field = TEST_FIELD
        };

        var queryBuilder = new QueryBuilder<TestClass>();

        Assert.Throws<ArgumentException>(() => queryBuilder.WithQuery(query));
    }

    [Fact]
    public void Should_Add_Multiple_Queries()
    {
        var queries = new ContentQuery[] { EQUALS, INCLUDES };

        var queryBuilder = new QueryBuilder<TestClass>();
        queryBuilder.WithQueries(queries);
        var queryString = queryBuilder.Build();

        string expectedEqualsString = GetExpectedStringForEquals(EQUALS);
        string expectedIncludesString = GetExpectedStringForIncludes(INCLUDES);

        Assert.Contains(expectedEqualsString, queryString);
        Assert.Contains(expectedIncludesString, queryString);
    }

    private static string GetExpectedStringForIncludes(ContentQueryMultipleValues query)
    => $"{query.Field}[in]={string.Join("%2C", query.Value)}";

    private static string GetExpectedStringForEquals(ContentQuerySingleValue query)
    => $"{query.Field}={query.Value}";
}
