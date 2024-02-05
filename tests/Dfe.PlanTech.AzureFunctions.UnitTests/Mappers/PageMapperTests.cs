namespace Dfe.PlanTech.AzureFunctions.UnitTests;

public class PageMapperTests : BaseMapperTests
{
    private const string PageId = "Header Id";

    private readonly PageMapper _mapper;
    private readonly CmsDbContext _db = Substitute.For<CmsDbContext>();
    private readonly ILogger<PageMapper> _logger;
    private readonly DbSet<PageContentDbEntity> _pageContentsDbSet = Substitute.For<DbSet<PageContentDbEntity>>();
    private readonly List<PageContentDbEntity> _attachedPageContents = new(10);

    private readonly Faker _faker = new();

    public PageMapperTests()
    {
        _logger = Substitute.For<ILogger<PageMapper>>();
        _mapper = new PageMapper(_db, _logger, JsonOptions);

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

        var mapped = _mapper.MapEntity(payload);

        Assert.NotNull(mapped);

        var concrete = mapped as PageDbEntity;
        Assert.NotNull(concrete);

        Assert.Equal(PageId, concrete.Id);

        foreach (var beforeTitle in beforeTitleContent)
        {
            var matching = _attachedPageContents.FirstOrDefault(pageContent => pageContent.BeforeContentComponentId == beforeTitle.Sys.Id);
            Assert.NotNull(matching);
        }

        foreach (var afterTitleContent in content)
        {
            var matching = _attachedPageContents.FirstOrDefault(pageContent => pageContent.ContentComponentId == afterTitleContent.Sys.Id);
            Assert.NotNull(matching);
        }
    }

    private CmsWebHookSystemDetailsInnerContainer GenerateReference()
    => new() { Sys = new() { Id = _faker.Random.Guid().ToString() } };
}