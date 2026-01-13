using Contentful.Core.Search;
using Dfe.PlanTech.Core.Contentful.Options;
using Dfe.PlanTech.Core.Contentful.Queries;

namespace Dfe.PlanTech.Core.UnitTests.Contentful.Queries;

public class QueryBuildersTests
{
    private const string ContentType = "testType";

    private sealed class DummyEntry { } // T for QueryBuilder<T>

    [Fact]
    public void ByContentType_Throws_When_Null_Or_Empty()
    {
        Assert.Throws<ArgumentNullException>(() => QueryBuilders.ByContentType<DummyEntry>(null));
        Assert.Throws<ArgumentNullException>(() => QueryBuilders.ByContentType<DummyEntry>(""));
    }

    [Fact]
    public void ByContentType_Sets_ContentType_Filter()
    {
        var qb = QueryBuilders.ByContentType<DummyEntry>(ContentType);
        var kv = qb.QueryStringValues();

        var ctPair = kv.SingleOrDefault(p => p.Key == "content_type");
        Assert.Equal(ContentType, ctPair.Value);
    }

    [Fact]
    public void WithInclude_Sets_Include()
    {
        var qb = new QueryBuilder<DummyEntry>();
        var opts = new GetEntriesOptions(include: 5);

        qb = qb.WithInclude(opts);

        var kv = qb.QueryStringValues();
        Assert.Contains(kv, p => p.Key == "include" && p.Value == "5");
    }

    [Fact]
    public void WithSelect_Adds_Select_Comma_Joined()
    {
        var qb = new QueryBuilder<DummyEntry>();
        var opts = new GetEntriesOptions(include: 1) { Select = new[] { "fields.title", "sys" } };

        qb = qb.WithSelect(opts);

        var kv = qb.QueryStringValues();
        Assert.Contains(kv, p => p.Key == "select" && p.Value == "fields.title,sys");
    }

    [Fact]
    public void WithSelect_NoOp_When_Select_Null()
    {
        var qb = new QueryBuilder<DummyEntry>();
        var opts = new GetEntriesOptions(include: 1) { Select = null };

        qb = qb.WithSelect(opts);

        var kv = qb.QueryStringValues();
        Assert.DoesNotContain(kv, p => p.Key == "select");
    }

    [Fact]
    public void BuildQueryBuilder_Composes_ContentType_Include_And_Select()
    {
        var opts = new GetEntriesOptions(include: 3) { Select = new[] { "fields.slug", "sys" } };

        var qb = QueryBuilders.BuildQueryBuilder<DummyEntry>(ContentType, opts);

        var kv = qb.QueryStringValues();
        Assert.Contains(kv, p => p.Key == "content_type" && p.Value == ContentType);
        Assert.Contains(kv, p => p.Key == "include" && p.Value == "3");
        Assert.Contains(kv, p => p.Key == "select" && p.Value == "fields.slug,sys");
    }

    [Fact]
    public void WithQuery_Throws_For_Unknown_ContentfulQuery_Type()
    {
        var qb = new QueryBuilder<DummyEntry>();
        var unknown = new UnknownQuery(); // not Single/Multiple -> should throw

        var ex = Assert.Throws<ArgumentException>(() => qb.WithQuery(unknown));
        Assert.Contains("Could not find correct query builder", ex.Message);
    }

    [Fact]
    public void WithQueries_IEnumerable_Adds_All_Provided_Filters()
    {
        var qb = new QueryBuilder<DummyEntry>();
        var q1 = new ContentfulQuerySingleValue { Field = "fields.title", Value = "Alpha" };
        var q2 = new ContentfulQuerySingleValue { Field = "fields.slug", Value = "intro" };

        qb = qb.WithQueries(new[] { q1, q2 });

        var kv = qb.QueryStringValues();
        // We expect *some* entries referencing the fields and values we passed in
        Assert.Contains(kv, p => p.Key.Contains("fields.title") && p.Value.Contains("Alpha", StringComparison.Ordinal));
        Assert.Contains(kv, p => p.Key.Contains("fields.slug") && p.Value.Contains("intro", StringComparison.Ordinal));
    }

    [Fact]
    public void WithQueries_FromOptions_Applies_When_Queries_Not_Null()
    {
        var qb = new QueryBuilder<DummyEntry>();
        var opts = new GetEntriesOptions(include: 1)
        {
            // NOTE: This property name must match your GetEntriesOptions.
            // Your QueryBuilders.WithQueries checks options.Queries (not Filters),
            // so we set Queries here.
            Queries = new List<ContentfulQuery>
            {
                new ContentfulQuerySingleValue { Field = "fields.category", Value = "security" }
            }
        };

        qb = qb.WithQueries(opts);

        var kv = qb.QueryStringValues();
        Assert.Contains(kv, p => p.Key.Contains("fields.category") && p.Value.Contains("security", StringComparison.Ordinal));
    }

    // A dummy ContentfulQuery to trigger the default switch branch in WithQuery
    private sealed class UnknownQuery : ContentfulQuery { }

}
