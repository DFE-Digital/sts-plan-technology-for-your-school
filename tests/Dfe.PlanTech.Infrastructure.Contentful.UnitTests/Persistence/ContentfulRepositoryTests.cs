using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contentful.Core;
using Contentful.Core.Models;
using Contentful.Core.Search;
using Moq;
using Sts.PlanTech.Infrastructure.Contentful.Persistence;
using Sts.PlanTech.Infrastructure.Contentful.Tests;
using Sts.PlanTech.Infrastructure.Persistence;

namespace Dfe.PlanTech.Infrastructure.Contentful.UnitTests.Persistence
{
    public class ContentfulRepositoryTests
    {
        private Mock<IContentfulClient> _clientMock = new Mock<IContentfulClient>();

        public ContentfulRepositoryTests()
        {
            _clientMock.Setup(client => client.GetEntries<object>(It.IsAny<QueryBuilder<object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((QueryBuilder<object> query, CancellationToken token) => new ContentfulCollection<object>());

            _clientMock.Setup(client => client.GetEntry<TestClass>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, string etag, QueryBuilder<TestClass> query, CancellationToken token) => {
                var result = new TestClass();
                return new ContentfulResult<TestClass>(etag, result);
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
        public async Task Should_Call_Client_Method_When_Using_GetEntityById()
        {
            IContentRepository repository = new ContentfulRepository(_clientMock.Object);

            var result = await repository.GetEntityById<TestClass>("id");

            Assert.NotNull(result);
        }
    }
}