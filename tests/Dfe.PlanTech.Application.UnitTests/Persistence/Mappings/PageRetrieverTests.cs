using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Application.UnitTests.Persistence.Mappings;

public class PageRetrieverTests : BaseMapperTests
{
    private const string ExistingPageId = "existing-page";
    private readonly PageRetriever _retriever;

    private readonly List<PageDbEntity> _pages = [];

    private readonly PageDbEntity _existingPage = new() { Id = ExistingPageId, };

    public PageRetrieverTests()
    {
        MockDatabaseCollection(_pages);

        _retriever = new PageRetriever(DatabaseHelper);
        _pages.Add(_existingPage);
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
