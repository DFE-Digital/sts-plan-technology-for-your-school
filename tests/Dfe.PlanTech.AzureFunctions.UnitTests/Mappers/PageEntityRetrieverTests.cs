using System.Linq.Expressions;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class PageEntityRetrieverTests
{
  private const string ExistingPageId = "existing-page";
  private readonly CmsDbContext _db = Substitute.For<CmsDbContext>();
  private readonly PageEntityRetriever _retriever;

  private readonly List<PageDbEntity> _pages = [];

  private readonly PageDbEntity _existingPage = new() { Id = ExistingPageId, };

  public PageEntityRetrieverTests()
  {
    _retriever = new PageEntityRetriever(_db);

    _pages.Add(_existingPage);

    MockPageDbSet();
  }

  private void MockPageDbSet()
  {
    IQueryable<PageDbEntity> queryable = _pages.AsQueryable();

    var asyncProvider = new AsyncQueryProvider<PageDbEntity>(queryable.Provider);

    var mockPageDataSet = Substitute.For<DbSet<PageDbEntity>, IQueryable<PageDbEntity>>();
    ((IQueryable<PageDbEntity>)mockPageDataSet).Provider.Returns(asyncProvider);
    ((IQueryable<PageDbEntity>)mockPageDataSet).Expression.Returns(queryable.Expression);
    ((IQueryable<PageDbEntity>)mockPageDataSet).ElementType.Returns(queryable.ElementType);
    ((IQueryable<PageDbEntity>)mockPageDataSet).GetEnumerator().Returns(queryable.GetEnumerator());
    _db.Pages = mockPageDataSet;
  }

  [Theory]
  [InlineData(ExistingPageId, true)]
  [InlineData("not-existing-entity-id", false)]
  public async Task Should_Find_Existing_Entity_If_Any(string pageId, bool shouldBeFound)
  {
    var incomingPage = new PageDbEntity() { Id = pageId };
    var result = await _retriever.GetExistingDbEntity(incomingPage, CancellationToken.None);

    var resultFound = result != null;

    Assert.Equal(shouldBeFound, resultFound);
  }
}