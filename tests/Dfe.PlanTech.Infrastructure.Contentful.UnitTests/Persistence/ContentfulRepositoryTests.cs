using System.Web;
using Contentful.Core;
using Contentful.Core.Models;
using Contentful.Core.Search;
using Dfe.PlanTech.Infrastructure.Contentful.Helpers;
using Dfe.PlanTech.Infrastructure.Contentful.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Dfe.PlanTech.Infrastructure.Contentful.UnitTests.Persistence
{
    public class ContentfulRepositoryTests
    {
        private readonly IContentfulClient _clientSubstitute = Substitute.For<IContentfulClient>();

        private readonly List<TestClass> _substituteData = new() {
            new TestClass(), new TestClass("testId"), new TestClass("anotherId"), new TestClass("abcd1234"), new TestClass("duplicateId"), new TestClass("duplicateId")
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
                    Items = items
                };

                return Task.FromResult(collection);
            });

            _clientSubstitute.GetEntries(Arg.Any<QueryBuilder<OtherTestClass>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var collection = new ContentfulCollection<OtherTestClass>
                {
                    Items = Enumerable.Empty<OtherTestClass>()
                };

                return Task.FromResult(collection);
            });

            _clientSubstitute.GetEntry<TestClass>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((CallInfo) =>
            {
                string id = string.Empty;
                string etag = string.Empty;
                var matching = _substituteData.FirstOrDefault(test => test.Id == id);
                if (matching == null)
                    return Task.FromResult(new ContentfulResult<TestClass>());

                return Task.FromResult(new ContentfulResult<TestClass>(etag, matching));
            });
        }

        [Fact]
        public async Task Should_Call_Client_Method_When_Using_GetEntities()
        {
            ContentfulRepository repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute);

            var result = await repository.GetEntities<TestClass>();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_CallClientMethod_When_Using_GetEntityById()
        {
            ContentfulRepository repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute);

            var result = await repository.GetEntityById<TestClass>("testId");

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetEntities_Should_ReturnItems_When_ClassMatches()
        {
            ContentfulRepository repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute);

            var result = await repository.GetEntities<TestClass>();

            Assert.NotNull(result);
            Assert.Equal(result, _substituteData);
        }


        [Fact]
        public async Task GetEntities_Should_ReturnEmptyIEnumerable_When_NoDataFound()
        {
            ContentfulRepository repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute);

            var result = await repository.GetEntities<OtherTestClass>();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetEntityById_Should_FindMatchingItem_When_IdMatches()
        {
            var testId = "testId";

            ContentfulRepository repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute);

            var result = await repository.GetEntityById<TestClass>(testId);

            Assert.NotNull(result);
            Assert.Equal(result.Id, testId);
        }

        [Fact]
        public async Task GetEntityById_Should_ThrowException_When_IdIsNull()
        {
            ContentfulRepository repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute);

            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.GetEntityById<TestClass>(null));
        }

        [Fact]
        public async Task GetEntityById_Should_ThrowException_When_IdIsEmpty()
        {
            ContentfulRepository repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute);

            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.GetEntityById<TestClass>(""));
        }

        [Fact]
        public async Task Should_ReturnNull_When_IdNotFound()
        {
            ContentfulRepository repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute);

            var result = await repository.GetEntityById<TestClass>("not a real id");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetEntityById_Should_Throw_GetEntitiesIDException_When_DuplicateIds()
        {
            var testId = "duplicateId";

            ContentfulRepository repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute);

            await Assert.ThrowsAsync<GetEntitiesException>(() => repository.GetEntityById<TestClass>(testId));
        }

        [Fact]
        public async Task GetEntityById_Should_Throw_GetEntitiesIDException_With_Correct_Exception_Message_When_DuplicateIds()
        {
            var testId = "duplicateId";

            ContentfulRepository repository = new ContentfulRepository(new NullLoggerFactory(), _clientSubstitute);

            var exceptionMessage = await Assert.ThrowsAsync<GetEntitiesException>(() => repository.GetEntityById<TestClass>(testId));
            Assert.Equal("Found more than 1 entity with id duplicateId", exceptionMessage.Message);
        }
    }
}
