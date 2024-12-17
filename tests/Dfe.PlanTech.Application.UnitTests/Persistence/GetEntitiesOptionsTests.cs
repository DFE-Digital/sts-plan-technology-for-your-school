using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Infrastructure.Application.Models;

namespace Dfe.PlanTech.Application.UnitTests.Persistence;

public class GetEntitiesOptionsTests
{
    private readonly Dictionary<string, GetEntitiesOptions> _testData;

    public GetEntitiesOptionsTests()
    {
        _testData = new Dictionary<string, GetEntitiesOptions>
        {
            { "Empty", new GetEntitiesOptions() },
            { "Include", new GetEntitiesOptions(include: 4) },
            {
                "Select", new GetEntitiesOptions()
                {
                    Select = ["field.intros", "field.sys"]
                }
            },
            {
                "Query", new GetEntitiesOptions(queries:
                [
                    new ContentQueryEquals() { Field = "slug", Value = "/" },
                    new ContentQueryEquals() { Field = "id", Value = "1234" },
                    new ContentQueryIncludes() { Field = "toinclude", Value = ["value1", "value2"] }
                ])
            },
            {
                "Combined", new GetEntitiesOptions()
                {
                    Select = ["field.intros", "field.sys"],
                    Queries =
                    [
                        new ContentQueryEquals() { Field = "slug", Value = "/test" },
                    ],
                    Include = 6
                }
            }
        };
    }

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

    [Theory]
    [InlineData("Empty", ":Include=2")]
    [InlineData("Select", ":Select=[field.intros,field.sys]:Include=2")]
    [InlineData("Include", ":Include=4")]
    [InlineData("Query", ":Queries=[slug=/,id=1234,toinclude=[value1,value2]]:Include=2")]
    [InlineData("Combined", ":Select=[field.intros,field.sys]:Queries=[slug=/test]:Include=6")]
    public void Should_Serialise_Options_Into_Suitable_Redis_Format(string testDataKey, string expectedValue)
    {
        var testData = _testData[testDataKey];
        var serialized = testData.SerializeToRedisFormat();
        Assert.Equal(expectedValue, serialized);
    }

}
