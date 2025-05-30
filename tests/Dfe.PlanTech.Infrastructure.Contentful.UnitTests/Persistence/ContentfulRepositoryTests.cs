using System.Web;
using Contentful.Core;
using Contentful.Core.Models;
using Contentful.Core.Search;
using Dfe.PlanTech.Application.Options;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Helpers;
using Dfe.PlanTech.Infrastructure.Contentful.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Dfe.PlanTech.Infrastructure.Contentful.UnitTests.Persistence
{
    public class ContentfulRepositoryTests
    {
        private readonly IContentfulClient _clientSubstitute = Substitute.For<IContentfulClient>();
        private readonly IHostEnvironment _hostEnvironmentMock = Substitute.For<IHostEnvironment>();
        private readonly QueryBuilder<TestClass> _queryBuilderMock = Substitute.For<QueryBuilder<TestClass>>();

        private readonly IOptions<AutomatedTestingOptions> _automatedTestingOptions =
            Substitute.For<IOptions<AutomatedTestingOptions>>();

        private readonly List<TestClass> _substituteData = new()
        {
            new TestClass(),
            new TestClass("testId"),
            new TestClass("anotherId"),
            new TestClass("abcd1234"),
            new TestClass("duplicateId"),
            new TestClass("duplicateId")
        };

        public ContentfulRepositoryTests()
        {
            _clientSubstitute.GetEntries(Arg.Any<QueryBuilder<TestClass>>(), Arg.Any<CancellationToken>())
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
                    Errors = new List<ContentfulError>()
                };
                return Task.FromResult(collection);
            });

            _clientSubstitute.GetEntries(Arg.Any<QueryBuilder<OtherTestClass>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new ContentfulCollection<OtherTestClass> { Items = new List<OtherTestClass>(), Errors = new List<ContentfulError>() }));

            _clientSubstitute.GetEntry<TestClass>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns((CallInfo) =>
                {
                    string id = string.Empty;
                    var matching = _substituteData.FirstOrDefault(test => test.Id == id);
                    return Task.FromResult(matching == null ? new ContentfulResult<TestClass>() : new ContentfulResult<TestClass>("etag", matching));
                });
        }

        [Fact]
        public async Task Should_Call_Client_Method_When_Using_GetEntities()
        {
            var repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute, _hostEnvironmentMock, _automatedTestingOptions);
            var result = await repository.GetEntities<TestClass>();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_CallClientMethod_When_Using_GetEntityById()
        {
            var repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute, _hostEnvironmentMock, _automatedTestingOptions);
            var result = await repository.GetEntityById<TestClass>("testId");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetEntities_Should_ReturnItems_When_ClassMatches()
        {
            var repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute, _hostEnvironmentMock, _automatedTestingOptions);
            var result = await repository.GetEntities<TestClass>();
            Assert.NotNull(result);
            Assert.Equal(_substituteData, result);
        }

        [Fact]
        public async Task GetEntities_Should_ReturnEmptyIEnumerable_When_NoDataFound()
        {
            var repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute, _hostEnvironmentMock, _automatedTestingOptions);
            var result = await repository.GetEntities<OtherTestClass>();
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPaginatedEntities_Should_ReturnItems_When_ClassMatches()
        {
            var repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute, _hostEnvironmentMock, _automatedTestingOptions);
            var result = await repository.GetPaginatedEntities<TestClass>(new GetEntitiesOptions());
            Assert.NotNull(result);
            Assert.Equal(_substituteData, result);
        }

        [Fact]
        public async Task GetPaginatedEntities_Should_ReturnEmptyIEnumerable_When_NoDataFound()
        {
            var repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute, _hostEnvironmentMock, _automatedTestingOptions);
            var result = await repository.GetPaginatedEntities<OtherTestClass>(new GetEntitiesOptions());
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetEntitiesCount_ReturnsCorrectCount()
        {
            var repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute, _hostEnvironmentMock, _automatedTestingOptions);
            var expectedCount = 42;

            _clientSubstitute.GetEntries(Arg.Any<QueryBuilder<RecommendationChunk>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new ContentfulCollection<RecommendationChunk> { Items = new List<RecommendationChunk>() { new RecommendationChunk() }, Total = 42, Errors = new List<ContentfulError>() }));

            var result = await repository.GetEntitiesCount<RecommendationChunk>();

            Assert.Equal(expectedCount, result);
        }

        [Fact]
        public async Task GetEntityById_Should_FindMatchingItem_When_IdMatches()
        {
            var testId = "testId";
            var repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute, _hostEnvironmentMock, _automatedTestingOptions);
            var result = await repository.GetEntityById<TestClass>(testId);
            Assert.NotNull(result);
            Assert.Equal(result.Id, testId);
        }

        [Fact]
        public async Task GetEntityById_Should_ThrowException_When_IdIsNull()
        {
            var repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute, _hostEnvironmentMock, _automatedTestingOptions);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.GetEntityById<TestClass>(null));
        }

        [Fact]
        public async Task GetEntityById_Should_ThrowException_When_IdIsEmpty()
        {
            var repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute, _hostEnvironmentMock, _automatedTestingOptions);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.GetEntityById<TestClass>(""));
        }

        [Fact]
        public async Task Should_ReturnNull_When_IdNotFound()
        {
            var repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute, _hostEnvironmentMock, _automatedTestingOptions);
            var result = await repository.GetEntityById<TestClass>("not a real id");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetEntityById_Should_Throw_GetEntitiesIDException_When_DuplicateIds()
        {
            var testId = "duplicateId";
            var repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute, _hostEnvironmentMock, _automatedTestingOptions);
            await Assert.ThrowsAsync<GetEntitiesException>(() => repository.GetEntityById<TestClass>(testId));
        }

        [Fact]
        public async Task GetEntityById_Should_Throw_GetEntitiesIDException_With_Correct_Exception_Message_When_DuplicateIds()
        {
            var testId = "duplicateId";
            var repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute, _hostEnvironmentMock, _automatedTestingOptions);
            var exception = await Assert.ThrowsAsync<GetEntitiesException>(() => repository.GetEntityById<TestClass>(testId));
            Assert.Equal("Found more than 1 entity with id duplicateId", exception.Message);
        }
    }
}
