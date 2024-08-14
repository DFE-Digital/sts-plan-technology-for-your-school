using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class PageEntityUpdaterTests
{
    private const string PageId = "page-id";

    private readonly CmsDbContext _db = Substitute.For<CmsDbContext>();
    private readonly ILogger<PageEntityUpdater> _logger = Substitute.For<ILogger<PageEntityUpdater>>();
    private readonly PageEntityUpdater _updater;

    private readonly List<PageContentDbEntity> _pageContents = [
      new PageContentDbEntity()
      {
          ContentComponentId = "A",
          Order = 2,
          PageId = PageId,
          Id = 1,
      },
        new PageContentDbEntity()
        {
            ContentComponentId = "B",
            Order = 1,
            PageId = PageId,
            Id = 2
        },
        new PageContentDbEntity()
        {
            BeforeContentComponentId = "D",
            Order = 2,
            PageId = PageId,
            Id = 3
        },
        new PageContentDbEntity()
        {
            BeforeContentComponentId = "E",
            Order = 1,
            PageId = PageId,
            Id = 4
        }
    ];

    public PageEntityUpdaterTests()
    {
        _updater = new PageEntityUpdater(_logger, _db);
        IQueryable<PageContentDbEntity> queryable = _pageContents.AsQueryable();

        var asyncProvider = new AsyncQueryProvider<PageContentDbEntity>(queryable.Provider);

        var mockPageDataSet = Substitute.For<DbSet<PageContentDbEntity>, IQueryable<PageContentDbEntity>>();
        ((IQueryable<PageContentDbEntity>)mockPageDataSet).Provider.Returns(asyncProvider);
        ((IQueryable<PageContentDbEntity>)mockPageDataSet).Expression.Returns(queryable.Expression);
        ((IQueryable<PageContentDbEntity>)mockPageDataSet).ElementType.Returns(queryable.ElementType);
        ((IQueryable<PageContentDbEntity>)mockPageDataSet).GetEnumerator().Returns(queryable.GetEnumerator());
        _db.PageContents = mockPageDataSet;

        _db.PageContents.When(pc => pc.RemoveRange(Arg.Any<IEnumerable<PageContentDbEntity>>()))
                        .Do((callinfo) =>
                        {
                            var pageContentsToRemove = callinfo.ArgAt<IEnumerable<PageContentDbEntity>>(0);
                            _pageContents.RemoveAll(pc => pageContentsToRemove.Contains(pc));
                        });

        _db.PageContents.When(pc => pc.Remove(Arg.Any<PageContentDbEntity>()))
                .Do((callinfo) =>
                {
                    var pageContentsToRemove = callinfo.ArgAt<PageContentDbEntity>(0);
                    _pageContents.Remove(pageContentsToRemove);
                });

    }

    [Fact]
    public void Should_Error_If_Incorrect_ContentComponent_Types()
    {
        MappedEntity[] shouldErrorEntities = [new MappedEntity()
        {
            IncomingEntity = new ButtonDbEntity(),
            ExistingEntity = new PageDbEntity(),
            CmsEvent = CmsEvent.SAVE
        },
            new MappedEntity()
            {
                IncomingEntity = new PageDbEntity(),
                ExistingEntity = new QuestionDbEntity(),
                CmsEvent = CmsEvent.SAVE
            },
            new MappedEntity()
            {
                IncomingEntity = new SectionDbEntity(),
                ExistingEntity = new AnswerDbEntity(),
                CmsEvent = CmsEvent.SAVE
            }];

        foreach (var errorableEntity in shouldErrorEntities)
        {
            Assert.ThrowsAny<InvalidCastException>(() => _updater.UpdateEntityConcrete(errorableEntity));
        }
    }

    [Fact]
    public void Should_AddOrUpdate_NewAndExistingPageComponents()
    {
        var addedEntity = new PageContentDbEntity()
        {
            ContentComponentId = "C",
            Order = 3
        };

        var mappedEntity = new MappedEntity()
        {
            IncomingEntity = new PageDbEntity()
            {
                AllPageContents = [.. _pageContents.Select(InverseOrder), addedEntity],
                Id = "page-id"
            },
            ExistingEntity = new PageDbEntity()
            {
                AllPageContents = [.. _pageContents]
            },
            CmsEvent = CmsEvent.SAVE
        };

        var result = _updater.UpdateEntityConcrete(mappedEntity);

        var unboxedExistingEntity = (PageDbEntity)mappedEntity.ExistingEntity!;

        Assert.Contains(addedEntity, unboxedExistingEntity.AllPageContents);

        var aContent = unboxedExistingEntity.AllPageContents.FirstOrDefault(pc => pc.ContentComponentId == "A");
        var bContent = unboxedExistingEntity.AllPageContents.FirstOrDefault(pc => pc.ContentComponentId == "B");
        var dContent = unboxedExistingEntity.AllPageContents.FirstOrDefault(pc => pc.BeforeContentComponentId == "D");
        var eContent = unboxedExistingEntity.AllPageContents.FirstOrDefault(pc => pc.BeforeContentComponentId == "E");

        Assert.Equivalent(1, aContent!.Order);
        Assert.Equivalent(2, bContent!.Order);
        Assert.Equivalent(1, dContent!.Order);
        Assert.Equivalent(2, eContent!.Order);
    }

    [Fact]
    public void Should_Delete_Removed_Entities()
    {
        var removedEntity = _pageContents.First();

        var pageId = "page-id";

        var existingPage = new PageDbEntity()
        {
            AllPageContents = [.. _pageContents],
            Id = pageId
        };

        var incomingPage = new PageDbEntity()
        {
            AllPageContents = [.. _pageContents.Skip(1)],
            Id = pageId
        };

        var mappedEntity = new MappedEntity()
        {
            IncomingEntity = incomingPage,
            ExistingEntity = existingPage,
            CmsEvent = CmsEvent.SAVE
        };

        _updater.UpdateEntityConcrete(mappedEntity);

        Assert.DoesNotContain(removedEntity, existingPage.AllPageContents);
        Assert.DoesNotContain(removedEntity, _pageContents);
    }

    [Fact]
    public void Should_Delete_Duplicate_PageContents()
    {
        var pageId = "page-id";

        var incomingPage = new PageDbEntity()
        {
            AllPageContents = [.. _pageContents],
            Id = pageId
        };

        List<PageContentDbEntity> copies = [.. _pageContents];
        foreach (var copy in copies)
        {
            copy.Id *= 10;
        }

        _pageContents.AddRange(copies);

        var existingPage = new PageDbEntity()
        {
            AllPageContents = [.. _pageContents],
            Id = pageId
        };

        var mappedEntity = new MappedEntity()
        {
            IncomingEntity = incomingPage,
            ExistingEntity = existingPage,
            CmsEvent = CmsEvent.SAVE
        };

        _updater.UpdateEntityConcrete(mappedEntity);

        Assert.Equal(incomingPage.AllPageContents.Count, existingPage.AllPageContents.Count);
        Assert.Equal(incomingPage.AllPageContents.Count, _pageContents.Count);
    }
    private static readonly Func<PageContentDbEntity, PageContentDbEntity> InverseOrder
      = pc => new PageContentDbEntity()
      {
          BeforeContentComponentId = pc.BeforeContentComponentId,
          ContentComponentId = pc.ContentComponentId,
          PageId = pc.PageId,
          Order = pc.Order == 1 ? 2 : 1
      };
}
