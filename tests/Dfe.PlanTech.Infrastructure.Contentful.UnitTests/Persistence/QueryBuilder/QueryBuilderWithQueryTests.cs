using Sts.PlanTech.Infrastructure.Contentful.Persistence;
using Sts.PlanTech.Infrastructure.Persistence.Querying;
using Contentful.Core.Search;

namespace Dfe.PlanTech.Infrastructure.Contentful.UnitTests;

public class QueryBuilderWithQueryTests
{
    private const string TEST_VALUE = "TestValue";
    private const string TEST_FIELD = "TestField";

    [Fact]
    public void Should_Call_ContentQueryEquals_When_ClassIs_ContentQueryEquals()
    {

        var query = new ContentQueryEquals()
        {
            Value = TEST_VALUE,
            Field = TEST_FIELD
        };

        var queryBuilder = new QueryBuilder<TestClass>();

        queryBuilder.WithQuery(query);

        var queryString = queryBuilder.Build();

        Assert.True(queryString.Contains($"{TEST_FIELD}={TEST_VALUE}"));
    }

    [Fact]
    public void Should_Call_ContentQueryIncludes_When_ClassIs_ContentQueryIncludes()
    {

        var query = new ContentQueryIncludes()
        {
            Value = new[] { TEST_VALUE, "OtherTestValue" },
            Field = TEST_FIELD
        };

        var queryBuilder = new QueryBuilder<TestClass>();

        queryBuilder.WithQuery(query);

        var queryString = queryBuilder.Build();

        var expected = $"{TEST_FIELD}[in]={string.Join("%2C", query.Value)}";
        Assert.True(queryString.Contains(expected), $"Expected \"{expected}\" but received \"{queryString}\"");
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
}
