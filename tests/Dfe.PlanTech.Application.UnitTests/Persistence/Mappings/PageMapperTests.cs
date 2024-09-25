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
    private readonly ILogger<PageMapper> _logger;
    private readonly ILogger<PageEntityUpdater> _entityUpdaterLogger = Substitute.For<ILogger<PageEntityUpdater>>();
    private readonly List<PageContentDbEntity> _attachedPageContents = new(10);
    private readonly List<PageContentDbEntity> _pageContents = [];
    private readonly Faker _faker = new();

    public PageMapperTests()
    {
        _logger = Substitute.For<ILogger<PageMapper>>();
        _mapper = new PageMapper(new PageEntityRetriever(DatabaseHelper), new PageEntityUpdater(_entityUpdaterLogger, DatabaseHelper), Logger, JsonOptions, DatabaseHelper);

        var queryablePageContents = _pageContents.AsQueryable();
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
