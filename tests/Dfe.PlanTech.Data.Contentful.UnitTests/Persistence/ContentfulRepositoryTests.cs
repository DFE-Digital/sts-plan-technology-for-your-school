using System.Web;
using Contentful.Core;
using Contentful.Core.Models;
using Contentful.Core.Search;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Options;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Options;
using Dfe.PlanTech.Data.Contentful.Persistence;
using Dfe.PlanTech.Data.Contentful.UnitTests.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Dfe.PlanTech.Data.Contentful.UnitTests.Persistence;

public class ContentfulRepositoryTests
{
    private readonly ILogger<ContentfulRepository> _logger = Substitute.For<
        ILogger<ContentfulRepository>
    >();
    private readonly IContentfulClient _clientSubstitute = Substitute.For<IContentfulClient>();
    private readonly IHostEnvironment _hostEnvironmentMock = Substitute.For<IHostEnvironment>();
    private readonly QueryBuilder<TestClass> _queryBuilderMock = Substitute.For<
        QueryBuilder<TestClass>
    >();

    private readonly IOptions<AutomatedTestingOptions> _automatedTestingOptions = Substitute.For<
        IOptions<AutomatedTestingOptions>
    >();

    private readonly List<TestClass> _substituteData = new()
    {
        new TestClass(),
        new TestClass("testId"),
        new TestClass("anotherId"),
        new TestClass("abcd1234"),
        new TestClass("duplicateId"),
        new TestClass("duplicateId"),
    };

    public ContentfulRepositoryTests()
    {
        _clientSubstitute
            .GetEntries(Arg.Any<QueryBuilder<TestClass>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                QueryBuilder<TestClass> query = (QueryBuilder<TestClass>)callInfo[0];
                var queryString = query.Build();
                var parsedQueryString = HttpUtility.ParseQueryString(queryString);
                var sysId = parsedQueryString.Get("sys.id");
                var items = _substituteData.AsEnumerable();

                if (sysId != null)
                {
                    items = items.Where(testData => testData.Id == sysId);
                }

                var collection = new ContentfulCollection<TestClass>
                {
                    Items = items.ToList(),
                    Errors = new List<ContentfulError>(),
                };
                return Task.FromResult(collection);
            });

        _clientSubstitute
            .GetEntries(Arg.Any<QueryBuilder<OtherTestClass>>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult(
                    new ContentfulCollection<OtherTestClass>
                    {
                        Items = new List<OtherTestClass>(),
                        Errors = new List<ContentfulError>()
                        {
                            new ContentfulError()
                            {
                                Details = new ContentfulErrorDetails
                                {
                                    Id = "E1",
                                    Type = "Test error",
                                },
                                SystemProperties = new SystemProperties { Id = "E1" },
                            },
                        },
                    }
                )
            );

        _clientSubstitute
            .GetEntry<TestClass>(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                (CallInfo) =>
                {
                    string id = string.Empty;
                    var matching = _substituteData.FirstOrDefault(test => test.Id == id);
                    return Task.FromResult(
                        matching == null
                            ? new ContentfulResult<TestClass>()
                            : new ContentfulResult<TestClass>("etag", matching)
                    );
                }
            );

        _automatedTestingOptions.Value.Returns(
            new AutomatedTestingOptions
            {
                Contentful = new() { IncludeTaggedContent = false, Tag = "e2e" },
            }
        );
    }

    [Fact]
    public async Task Should_Call_Client_Method_When_Using_GetEntities()
    {
        var repository = new ContentfulRepository(
            _logger,
            _clientSubstitute,
            _hostEnvironmentMock,
            _automatedTestingOptions
        );
        var result = await repository.GetEntriesAsync<TestClass>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Should_CallClientMethod_When_Using_GetEntityById()
    {
        var repository = new ContentfulRepository(
            _logger,
            _clientSubstitute,
            _hostEnvironmentMock,
            _automatedTestingOptions
        );
        var result = await repository.GetEntryByIdAsync<TestClass>("testId");
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetEntities_Should_ReturnItems_When_ClassMatches()
    {
        var repository = new ContentfulRepository(
            _logger,
            _clientSubstitute,
            _hostEnvironmentMock,
            _automatedTestingOptions
        );
        var result = await repository.GetEntriesAsync<TestClass>();
        Assert.NotNull(result);
        Assert.Equal(_substituteData, result);
    }

    [Fact]
    public async Task GetEntities_Should_ReturnEmptyIEnumerable_When_NoDataFound()
    {
        var repository = new ContentfulRepository(
            _logger,
            _clientSubstitute,
            _hostEnvironmentMock,
            _automatedTestingOptions
        );
        var result = await repository.GetEntriesAsync<OtherTestClass>();
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEntities_Should_LogError_When_NoDataFound()
    {
        var repository = new ContentfulRepository(
            _logger,
            _clientSubstitute,
            _hostEnvironmentMock,
            _automatedTestingOptions
        );
        await repository.GetEntriesAsync<OtherTestClass>();

        _logger
            .Received(1)
            .Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<IReadOnlyList<KeyValuePair<string, object>>>(state =>
                    state.Any(kv =>
                        kv.Key == "{OriginalFormat}"
                        && (string)kv.Value
                            == "Error retrieving one or more {EntryType} entries from Contentful:\n{Errors}"
                    )
                    && state.Any(kv =>
                        kv.Key == "EntryType" && (string)kv.Value == nameof(OtherTestClass)
                    )
                    && state.Any(kv =>
                        kv.Key == "Errors"
                        && ((IEnumerable<string>)kv.Value).Single() == "[Test error] E1"
                    )
                ),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
    }

    [Fact]
    public async Task GetPaginatedEntities_Should_ReturnItems_When_ClassMatches()
    {
        var repository = new ContentfulRepository(
            _logger,
            _clientSubstitute,
            _hostEnvironmentMock,
            _automatedTestingOptions
        );
        var result = await repository.GetPaginatedEntriesAsync<TestClass>(new GetEntriesOptions());
        Assert.NotNull(result);
        Assert.Equal(_substituteData, result);
    }

    [Fact]
    public async Task GetPaginatedEntities_Should_ReturnEmptyIEnumerable_When_NoDataFound()
    {
        var repository = new ContentfulRepository(
            _logger,
            _clientSubstitute,
            _hostEnvironmentMock,
            _automatedTestingOptions
        );
        var result = await repository.GetPaginatedEntriesAsync<OtherTestClass>(
            new GetEntriesOptions()
        );
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEntitiesCount_ReturnsCorrectCount()
    {
        var repository = new ContentfulRepository(
            _logger,
            _clientSubstitute,
            _hostEnvironmentMock,
            _automatedTestingOptions
        );
        var expectedCount = 42;

        _clientSubstitute
            .GetEntries(
                Arg.Any<QueryBuilder<RecommendationChunkEntry>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                Task.FromResult(
                    new ContentfulCollection<RecommendationChunkEntry>
                    {
                        Items = new List<RecommendationChunkEntry>()
                        {
                            new RecommendationChunkEntry(),
                        },
                        Total = 42,
                        Errors = new List<ContentfulError>(),
                    }
                )
            );

        var result = await repository.GetEntriesCountAsync<RecommendationChunkEntry>();

        Assert.Equal(expectedCount, result);
    }

    [Fact]
    public async Task GetEntityById_Should_FindMatchingItem_When_IdMatches()
    {
        var testId = "testId";
        var repository = new ContentfulRepository(
            _logger,
            _clientSubstitute,
            _hostEnvironmentMock,
            _automatedTestingOptions
        );
        var result = await repository.GetEntryByIdAsync<TestClass>(testId);
        Assert.NotNull(result);
        Assert.Equal(result.Id, testId);
    }

    [Fact]
    public async Task GetEntityById_Should_ThrowException_When_IdIsNull()
    {
        var repository = new ContentfulRepository(
            _logger,
            _clientSubstitute,
            _hostEnvironmentMock,
            _automatedTestingOptions
        );
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            repository.GetEntryByIdAsync<TestClass>(null)
        );
    }

    [Fact]
    public async Task GetEntityById_Should_ThrowException_When_IdIsEmpty()
    {
        var repository = new ContentfulRepository(
            _logger,
            _clientSubstitute,
            _hostEnvironmentMock,
            _automatedTestingOptions
        );
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            repository.GetEntryByIdAsync<TestClass>("")
        );
    }

    [Fact]
    public async Task Should_ReturnNull_When_IdNotFound()
    {
        var repository = new ContentfulRepository(
            _logger,
            _clientSubstitute,
            _hostEnvironmentMock,
            _automatedTestingOptions
        );
        var result = await repository.GetEntryByIdAsync<TestClass>("not a real id");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetEntityById_Should_Throw_GetEntitiesIDException_When_DuplicateIds()
    {
        var testId = "duplicateId";
        var repository = new ContentfulRepository(
            _logger,
            _clientSubstitute,
            _hostEnvironmentMock,
            _automatedTestingOptions
        );
        await Assert.ThrowsAsync<GetEntriesException>(() =>
            repository.GetEntryByIdAsync<TestClass>(testId)
        );
    }

    [Fact]
    public async Task GetEntityById_Should_Throw_GetEntitiesIDException_With_Correct_Exception_Message_When_DuplicateIds()
    {
        var testId = "duplicateId";
        var repository = new ContentfulRepository(
            _logger,
            _clientSubstitute,
            _hostEnvironmentMock,
            _automatedTestingOptions
        );
        var exception = await Assert.ThrowsAsync<GetEntriesException>(() =>
            repository.GetEntryByIdAsync<TestClass>(testId)
        );
        Assert.Equal("Found more than 1 entity with id duplicateId", exception.Message);
    }
}
