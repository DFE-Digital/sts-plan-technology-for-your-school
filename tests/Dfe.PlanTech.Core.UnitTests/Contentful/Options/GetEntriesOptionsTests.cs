using Dfe.PlanTech.Core.Contentful.Options;
using Dfe.PlanTech.Core.Contentful.Queries;

namespace Dfe.PlanTech.Core.UnitTests.Contentful.Options;

public class GetEntriesOptionsTests
{
    private static readonly string[] item = ["a", "b", "c"];

    [Fact]
    public void Defaults_Are_Correct()
    {
        var opts = new GetEntriesOptions();

        Assert.Equal(1, opts.Page); // default page
        Assert.Equal(2, opts.Include); // default include
        Assert.Null(opts.Limit);
        Assert.Null(opts.Select);
        Assert.Null(opts.Queries);
    }

    [Fact]
    public void Ctor_With_Include_Sets_Include()
    {
        var opts = new GetEntriesOptions(include: 5);
        Assert.Equal(5, opts.Include);
        Assert.Null(opts.Queries);
    }

    [Fact]
    public void Ctor_With_Queries_Sets_Queries_And_Leaves_Include_Default()
    {
        var queries = new List<ContentfulQuery>
        {
            new ContentfulQuerySingleValue { Field = "fields.slug", Value = "about" },
        };

        var opts = new GetEntriesOptions(queries);
        Assert.Equal(2, opts.Include); // default
        Assert.Same(queries, opts.Queries);
    }

    [Fact]
    public void Ctor_With_Include_And_Queries_Sets_Both()
    {
        var queries = new List<ContentfulQuery>
        {
            new ContentfulQuerySingleValue { Field = "fields.type", Value = "page" },
        };

        var opts = new GetEntriesOptions(include: 4, queries);
        Assert.Equal(4, opts.Include);
        Assert.Same(queries, opts.Queries);
    }

    [Fact]
    public void Serialize_Only_Include_When_No_Select_Or_Queries()
    {
        var opts = new GetEntriesOptions(include: 3);

        var s = opts.SerializeToRedisFormat();

        Assert.Equal(":Include=3", s);
    }

    [Fact]
    public void Serialize_Emits_Select_CommaJoined_In_Brackets()
    {
        var opts = new GetEntriesOptions(include: 2) { Select = new[] { "fields.title", "sys" } };

        var s = opts.SerializeToRedisFormat();

        Assert.Equal(":Include=2:Select=[fields.title,sys]", s);
    }

    [Fact]
    public void Serialize_Emits_Queries_Single_And_Multiple_Values_No_Trailing_Comma()
    {
        var opts = new GetEntriesOptions(include: 6)
        {
            Queries = new List<ContentfulQuery>
            {
                new ContentfulQuerySingleValue { Field = "fields.slug", Value = "intro" },
                new ContentfulQueryMultipleValues { Field = "fields.tags", Value = item },
            },
        };

        var s = opts.SerializeToRedisFormat();

        // Expected format:
        // :Include=6:Queries=[fields.slug=intro,fields.tags=[a,b,c]]
        Assert.StartsWith(":Include=6:Queries=[", s);
        Assert.EndsWith("]", s);

        Assert.Contains("fields.slug=intro", s);
        Assert.Contains("fields.tags=[a,b,c]", s);

        // Critical: ensure there is no trailing comma before the closing bracket
        Assert.DoesNotContain(",]", s);
    }

    [Fact]
    public void Serialize_Emits_Select_Then_Queries_When_Both_Present()
    {
        var opts = new GetEntriesOptions(include: 1)
        {
            Select = new[] { "fields.id" },
            Queries = new List<ContentfulQuery>
            {
                new ContentfulQuerySingleValue { Field = "fields.type", Value = "page" },
            },
        };

        var s = opts.SerializeToRedisFormat();

        Assert.Equal(":Include=1:Select=[fields.id]:Queries=[fields.type=page]", s);
    }

    [Fact]
    public void Serialize_Does_Not_Emit_Empty_Select_Or_Empty_Queries()
    {
        var opts = new GetEntriesOptions(include: 2)
        {
            Select = new string[0],
            Queries = new List<ContentfulQuery>(), // empty
        };

        var s = opts.SerializeToRedisFormat();

        Assert.Equal(":Include=2", s);
    }

    [Fact]
    public void Serialize_Ignores_Page_And_Limit()
    {
        var opts = new GetEntriesOptions(include: 9)
        {
            Select = new[] { "sys" },
            // init-only props; set Page & Limit but they should not appear in output
            // (Page has default 1; set another value just to be sure)
            // Using object initializer: they are 'init' so assign at creation
        };
        // Workaround to set init-only in test: create via new and object init
        opts = new GetEntriesOptions(include: 9)
        {
            Select = new[] { "sys" },
            Limit = 50,
            Page = 3,
        };

        var s = opts.SerializeToRedisFormat();

        Assert.Equal(":Include=9:Select=[sys]:Queries=[]".Replace(":Queries=[]", ""), s);
        // i.e., there should be no "Page=" nor "Limit=" in the string.
        Assert.DoesNotContain("Page=", s);
        Assert.DoesNotContain("Limit=", s);
    }
}
