using Contentful.Core;
using Contentful.Core.Models;
using Contentful.Core.Search;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Infrastructure.Contentful.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Web;

namespace Dfe.PlanTech.Infrastructure.Contentful.UnitTests.Persistence
{
    public class ContentfulRepositoryTests
    {
        private IContentfulClient _clientMock = Substitute.For<IContentfulClient>();

        private readonly List<TestClass> _mockData = new() {
            new TestClass(), new TestClass("testId"), new TestClass("anotherId"), new TestClass("abcd1234")
        };

        public ContentfulRepositoryTests()
        {
            _clientMock.GetEntries(Arg.Any<QueryBuilder<TestClass>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                QueryBuilder<TestClass> query = (QueryBuilder<TestClass>)callInfo[0];
                var queryString = query.Build();
                var parsedQueryString = HttpUtility.ParseQueryString(queryString);
                var sysId = parsedQueryString.Get("sys.id");

                var items = _mockData.AsEnumerable();

                if (sysId != null)
                {
                    items = items.Where(testData => testData.Id == sysId);
                }

                var collection = new ContentfulCollection<TestClass>
                {
                    Items = items
                };

                return Task.FromResult(collection);
            });

            _clientMock.GetEntries(Arg.Any<QueryBuilder<OtherTestClass>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var collection = new ContentfulCollection<OtherTestClass>
                {
                    Items = Enumerable.Empty<OtherTestClass>()
                };

                return Task.FromResult(collection);
            });

            _clientMock.GetEntry<TestClass>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((CallInfo) =>
            {
                string id = string.Empty;
                string etag = string.Empty;
                var matching = _mockData.FirstOrDefault(test => test.Id == id);
                if (matching == null) return Task.FromResult(new ContentfulResult<TestClass>());

                return Task.FromResult(new ContentfulResult<TestClass>(etag, matching));
            });
        }

        [Fact]
        public async Task Should_Call_Client_Method_When_Using_GetEntities()
        {
            IContentRepository repository = new ContentfulRepository(new NullLoggerFactory(), _clientMock);

            var result = await repository.GetEntities<TestClass>();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_CallClientMethod_When_Using_GetEntityById()
        {
            IContentRepository repository = new ContentfulRepository(new NullLoggerFactory(), _clientMock);

            var result = await repository.GetEntityById<TestClass>("testId");

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetEntities_Should_ReturnItems_When_ClassMatches()
        {
            IContentRepository repository = new ContentfulRepository(new NullLoggerFactory(), _clientMock);

            var result = await repository.GetEntities<TestClass>();

            Assert.NotNull(result);
            Assert.Equal(result, _mockData);
        }


        [Fact]
        public async Task GetEntities_Should_ReturnEmptyIEnumerable_When_NoDataFound()
        {
            IContentRepository repository = new ContentfulRepository(new NullLoggerFactory(), _clientMock);

            var result = await repository.GetEntities<OtherTestClass>();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetEntityById_Should_FindMatchingItem_When_IdMatches()
        {
            var testId = "testId";
            IContentRepository repository = new ContentfulRepository(new NullLoggerFactory(), _clientMock);

            var result = await repository.GetEntityById<TestClass>(testId);

            Assert.NotNull(result);
            Assert.Equal(result.Id, testId);
        }

        [Fact]
        public async Task GetEntityById_Should_ThrowException_When_IdIsNull()
        {
            IContentRepository repository = new ContentfulRepository(new NullLoggerFactory(), _clientMock);

            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.GetEntityById<TestClass>(null));
        }

        [Fact]
        public async Task GetEntityById_Should_ThrowException_When_IdIsEmpty()
        {
            IContentRepository repository = new ContentfulRepository(new NullLoggerFactory(), _clientMock);

            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.GetEntityById<TestClass>(""));
        }

        [Fact]
        public async Task Should_ReturnNull_When_IdNotFound()
        {
            IContentRepository repository = new ContentfulRepository(new NullLoggerFactory(), _clientMock);

            var result = await repository.GetEntityById<TestClass>("not a real id");

            Assert.Null(result);
        }
    }
}
