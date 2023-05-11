using Contentful.Core;
using Contentful.Core.Models;
using Contentful.Core.Search;
using Moq;
using Dfe.PlanTech.Infrastructure.Contentful.Persistence;
using Dfe.PlanTech.Application.Persistence.Interfaces;

namespace Dfe.PlanTech.Infrastructure.Contentful.UnitTests.Persistence
{
    public class ContentfulRepositoryTests
    {
        private readonly Mock<IContentfulClient> _clientMock = new();

        private readonly List<TestClass> _mockData = new() {
            new TestClass(), new TestClass("testId"), new TestClass("anotherId"), new TestClass("abcd1234")
        };

        public ContentfulRepositoryTests()
        {
            _clientMock.Setup(client => client.GetEntries<TestClass>(It.IsAny<QueryBuilder<TestClass>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((QueryBuilder<TestClass> query, CancellationToken token) =>
            {
                var collection = new ContentfulCollection<TestClass>
                {
                    Items = _mockData
                };

                return collection;
            });
            
            _clientMock.Setup(client => client.GetEntries<OtherTestClass>(It.IsAny<QueryBuilder<OtherTestClass>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((QueryBuilder<OtherTestClass> query, CancellationToken token) => {
                var collection = new ContentfulCollection<OtherTestClass>
                {
                    Items = Enumerable.Empty<OtherTestClass>()
                };

                return collection;
            });

            _clientMock.Setup(client => client.GetEntry<TestClass>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, string etag, QueryBuilder<TestClass> query, CancellationToken token) =>
            {
                var matching = _mockData.FirstOrDefault(test => test.Id == id);
                if (matching == null) return new ContentfulResult<TestClass>();

                return new ContentfulResult<TestClass>(etag, matching);
            });
        }

        [Fact]
        public async Task Should_Call_Client_Method_When_Using_GetEntities()
        {
            IContentRepository repository = new ContentfulRepository(_clientMock.Object);

            var result = await repository.GetEntities<TestClass>();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_CallClientMethod_When_Using_GetEntityById()
        {
            IContentRepository repository = new ContentfulRepository(_clientMock.Object);

            var result = await repository.GetEntityById<TestClass>("testId");

            Assert.NotNull(result);
        }
        
        [Fact]
        public async Task GetEntities_Should_ReturnItems_When_ClassMatches()
        {
            IContentRepository repository = new ContentfulRepository(_clientMock.Object);

            var result = await repository.GetEntities<TestClass>();

            Assert.NotNull(result);
            Assert.Equal(result, _mockData);
        }
        
        
        [Fact]
        public async Task GetEntities_Should_ReturnEmptyIEnumerable_When_NoDataFound()
        {
            IContentRepository repository = new ContentfulRepository(_clientMock.Object);

            var result = await repository.GetEntities<OtherTestClass>();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetEntityById_Should_FindMatchingItem_When_IdMatches()
        {
            var testId = "testId";
            IContentRepository repository = new ContentfulRepository(_clientMock.Object);

            var result = await repository.GetEntityById<TestClass>(testId);

            Assert.NotNull(result);
            Assert.Equal(result.Id, testId);
        }
        
        [Fact]
        public async Task GetEntityById_Should_ThrowException_When_IdIsNull()
        {
            IContentRepository repository = new ContentfulRepository(_clientMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.GetEntityById<TestClass>(null));
        }
        
        [Fact]
        public async Task GetEntityById_Should_ThrowException_When_IdIsEmpty()
        {
            IContentRepository repository = new ContentfulRepository(_clientMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.GetEntityById<TestClass>(""));
        }

        [Fact]
        public async Task Should_ReturnNull_When_IdNotFound()
        {
            IContentRepository repository = new ContentfulRepository(_clientMock.Object);

            var result = await repository.GetEntityById<TestClass>("not a real id");

            Assert.Null(result);
        }

    }
}
