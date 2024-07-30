using Bogus;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class PageMapperTests : BaseMapperTests
{
    private const string PageId = "Header Id";

    private readonly PageMapper _mapper;
    private readonly CmsDbContext _db = Substitute.For<CmsDbContext>();
    private readonly ILogger<PageMapper> _logger;
    private readonly ILogger<PageEntityUpdater> _entityUpdaterLogger = Substitute.For<ILogger<PageEntityUpdater>>();
    private readonly DbSet<PageContentDbEntity> _pageContentsDbSet;
    private readonly List<PageContentDbEntity> _attachedPageContents = new(10);
    private readonly List<PageContentDbEntity> _pageContents = [];
    private readonly Faker _faker = new();

    public PageMapperTests()
    {
        _logger = Substitute.For<ILogger<PageMapper>>();
        _mapper = new PageMapper(new PageEntityRetriever(_db), new PageEntityUpdater(_entityUpdaterLogger, _db), _logger, JsonOptions);

        var queryablePageContents = _pageContents.AsQueryable();
        var pageContentsAsyncProvider = new AsyncQueryProvider<PageContentDbEntity>(queryablePageContents.Provider);

        _pageContentsDbSet = Substitute.For<DbSet<PageContentDbEntity>, IQueryable<PageContentDbEntity>>();
        ((IQueryable<PageContentDbEntity>)_pageContentsDbSet).Provider.Returns(pageContentsAsyncProvider);
        ((IQueryable<PageContentDbEntity>)_pageContentsDbSet).Expression.Returns(queryablePageContents.Expression);
        ((IQueryable<PageContentDbEntity>)_pageContentsDbSet).ElementType.Returns(queryablePageContents.ElementType);
        ((IQueryable<PageContentDbEntity>)_pageContentsDbSet).GetEnumerator().Returns(queryablePageContents.GetEnumerator());

        _db.PageContents = _pageContentsDbSet;

        _pageContentsDbSet.WhenForAnyArgs(dbSet => dbSet.Attach(Arg.Any<PageContentDbEntity>()))
                    .Do(callinfo =>
                    {
                        var pageContent = callinfo.ArgAt<PageContentDbEntity>(0);
                        _attachedPageContents.Add(pageContent);
                    });
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
