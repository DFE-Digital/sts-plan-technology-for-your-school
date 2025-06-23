using Contentful.Core.Search;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Persistence;

namespace Dfe.PlanTech.Infrastructure.Contentful.UnitTests;

public class QueryBuilderSelectTests
{
    [Fact]
    public void Should_Add_Select_Properties()
    {
        var queryBuilder = new QueryBuilder<TestClass>();
        var options = new GetEntriesOptions()
        {
            Select = new[] { "fields.one", "fields.two" }
        };

        var joinedSelect = string.Join("%2C", options.Select);

        var expectedValue = $"?select={joinedSelect}";

        queryBuilder = queryBuilder.WithSelect(options);

        var built = queryBuilder.Build();

        Assert.Equal(expectedValue, built);
    }

    [Fact]
    public void Should_DoNothing_When_Select_Null()
    {
        var queryBuilder = new QueryBuilder<TestClass>();
        var options = new GetEntriesOptions()
        {
        };

        queryBuilder = queryBuilder.WithSelect(options);

        var built = queryBuilder.Build();

        Assert.Empty(built);
    }

}
