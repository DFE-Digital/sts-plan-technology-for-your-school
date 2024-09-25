using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Infrastructure.Application.Models;

namespace Dfe.PlanTech.Application.UnitTests.Persistence;

public class GetEntitiesOptionsTests
{
    [Fact]
    public void Should_Set_Include()
    {
        var include = 100;
        var options = new GetEntitiesOptions(include);

        Assert.Equal(include, options.Include);
    }

    [Fact]
    public void Should_Set_Queries()
    {
        var queries = new List<IContentQuery>(){
            new ContentQuery(){
                Field = "Testing"
            }
        };

        var options = new GetEntitiesOptions(queries);

        Assert.NotNull(options.Queries);
        Assert.Equal(queries, options.Queries);
    }

    [Fact]
    public void Should_Have_Default_Values()
    {
        var options = new GetEntitiesOptions();

        Assert.Equal(2, options.Include);
        Assert.Null(options.Queries);
    }

}
