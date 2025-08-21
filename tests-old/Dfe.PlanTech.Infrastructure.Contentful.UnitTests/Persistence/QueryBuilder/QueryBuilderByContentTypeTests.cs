using Dfe.PlanTech.Data.Contentful.Persistence;

namespace Dfe.PlanTech.Data.Contentful.UnitTests;

public class QueryBuilderByContentTypeTests
{
    private const string TEST_CONTENT_TYPE = "Testing";

    [Fact]
    public void Should_Create_When_String()
    {
        const string contentTypeId = TEST_CONTENT_TYPE;

        var queryBuilder = QueryBuilders.ByContentType<TestClass>(contentTypeId);

        Assert.NotNull(queryBuilder);
    }

    [Fact]
    public void Should_Create_Have_ContentType_Fulter_When_String()
    {
        const string contentTypeId = TEST_CONTENT_TYPE;

        var queryBuilder = QueryBuilders.ByContentType<TestClass>(contentTypeId);

        var built = queryBuilder.Build();

        Assert.True(built.Contains($"?content_type={TEST_CONTENT_TYPE}"), "Missing content type filter");
    }

    [Fact]
    public void Should_Error_When_String_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => QueryBuilders.ByContentType<TestClass>(null));
    }

    [Fact]
    public void Should_Error_When_String_Is_Empty()
    {
        Assert.Throws<ArgumentNullException>(() => QueryBuilders.ByContentType<TestClass>(""));
    }
}
