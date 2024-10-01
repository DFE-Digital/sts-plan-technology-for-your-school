using Bogus;
using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;

using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Persistence.Mappings;

public class PageMapperTests : BaseMapperTests<PageDbEntity, PageMapper>
{
    private const string PageId = "Header Id";

    private readonly PageMapper _mapper;
    private readonly ILogger<PageUpdater> _entityUpdaterLogger = Substitute.For<ILogger<PageUpdater>>();
    private readonly List<PageContentDbEntity> _attachedPageContents = new(10);
    private readonly List<PageContentDbEntity> _pageContents = [];
    private readonly List<PageDbEntity> _pages = [];
    private readonly Faker _faker = new();

    public PageMapperTests()
    {
        _mapper = new PageMapper(new PageRetriever(DatabaseHelper), new PageUpdater(_entityUpdaterLogger, DatabaseHelper), Logger, JsonOptions, DatabaseHelper);
        MockDatabaseCollection(_pages);
    }

    [Fact]
    public void Mapper_Should_Map_Page()
    {
        var beforeTitleContent = Enumerable.Range(1, 5).Select(i => GenerateReference()).ToArray();
        var content = Enumerable.Range(1, 5).Select(i => GenerateReference()).ToArray();

        var fields = new Dictionary<string, object?>()
        {
            ["beforeTitleContent"] = WrapWithLocalisation(beforeTitleContent),
            ["content"] = WrapWithLocalisation(content),
        };

        var payload = CreatePayload(fields, PageId);

        var mapped = _mapper.ToEntity(payload);

        Assert.NotNull(mapped);

        var concrete = mapped;
        Assert.NotNull(concrete);

        Assert.Equal(PageId, concrete.Id);

        ContentsExistAndAreCorrect(beforeTitleContent, content => content.BeforeContentComponentId);
        ContentsExistAndAreCorrect(content, content => content.ContentComponentId);
    }

    [Fact]
    public async Task GetExistingEntity_Should_Call_Retriever()
    {
        await _mapper.InvokeNonPublicAsyncMethod("GetExistingEntity",
            new object[] { new PageDbEntity(), CancellationToken.None });

        await DatabaseHelper.Database.Received(1).FirstOrDefaultAsync(Arg.Any<IQueryable<PageDbEntity>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PerformAdditionalMapping_Should_LogError_If_Id_NotFound()
    {
        var values = new Dictionary<string, object>();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _mapper.InvokeNonPublicAsyncMethod("PerformAdditionalMapping", new object?[] { values }));
    }

    [Fact]
    public void CreatePageContentEntity_Should_LogError_When_Inner_Not_String()
    {
        _mapper.InvokeNonPublicMethod("CreatePageContentEntity", new object?[] { 10, 0, true });
        var messages = Logger.ReceivedLogMessages().ToArray();

        Assert.Single(messages);
        Assert.Contains("Expected string but received", messages[0].Message);
    }
    private void ContentsExistAndAreCorrect(CmsWebHookSystemDetailsInnerContainer[] content, Func<PageContentDbEntity, string?> idSelector)
    {
        for (var index = 0; index < content.Length; index++)
        {
            var beforeTitle = content[index];
            var matching = _mapper.PageContents.FirstOrDefault(pc => idSelector(pc) == beforeTitle.Sys.Id);
            Assert.NotNull(matching);
            Assert.Equal(index, matching.Order);
        }
    }

    private CmsWebHookSystemDetailsInnerContainer GenerateReference()
    => new() { Sys = new() { Id = _faker.Random.Guid().ToString() } };
}
