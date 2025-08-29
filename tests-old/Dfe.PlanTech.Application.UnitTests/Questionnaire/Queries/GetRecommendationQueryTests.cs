using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Questionnaire.Queries
{
    public class GetRecommendationQueryTests
    {
        [Fact]
        public async Task GetChunksByPage_ThrowsExceptionOnRepositoryError()
        {
            var repository = Substitute.For<IContentRepository>();
            repository
                .When(repo => repo.GetPaginatedEntities<RecommendationChunk>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>()))
                .Throw(new Exception("Dummy Exception"));

            var getRecommendationQuery = new GetRecommendationQuery(repository);

            await Assert.ThrowsAsync<ContentfulDataUnavailableException>(
                async () => await getRecommendationQuery.GetChunksByPage(1, CancellationToken.None)
            );
        }

        [Fact]
        public async Task GetChunksByPage_ReturnsCorrectPaginationData()
        {
            var repository = Substitute.For<IContentRepository>();
            var expectedPage = 2;
            var expectedTotalCount = 100;
            var expectedChunks = new List<RecommendationChunk> { new RecommendationChunk(), new RecommendationChunk() };

            repository.GetEntitiesCount<RecommendationChunk>(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedTotalCount));

            repository.GetPaginatedEntities<RecommendationChunk>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<RecommendationChunk>>(expectedChunks));

            var getRecommendationQuery = new GetRecommendationQuery(repository);

            var result = await getRecommendationQuery.GetChunksByPage(expectedPage, CancellationToken.None);

            Assert.Equal(expectedPage, result.Pagination.Page);
            Assert.Equal(expectedTotalCount, result.Pagination.Total);
            Assert.Equal(expectedChunks, result.Chunks);
        }

        [Fact]
        public async Task GetChunksByPage_ReturnsEmptyListWhenNoData()
        {
            var repository = Substitute.For<IContentRepository>();

            repository.GetEntitiesCount<RecommendationChunk>(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(0));

            repository.GetPaginatedEntities<RecommendationChunk>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<RecommendationChunk>>(new List<RecommendationChunk>()));

            var getRecommendationQuery = new GetRecommendationQuery(repository);

            var result = await getRecommendationQuery.GetChunksByPage(1, CancellationToken.None);

            Assert.Empty(result.Chunks);
            Assert.Equal(1, result.Pagination.Page);
            Assert.Equal(0, result.Pagination.Total);
        }

        [Fact]
        public async Task GetChunksByPage_FirstPageReturnsCorrectly()
        {
            var repository = Substitute.For<IContentRepository>();
            var expectedChunks = new List<RecommendationChunk> { new RecommendationChunk(), new RecommendationChunk() };

            repository.GetEntitiesCount<RecommendationChunk>(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(50));

            repository.GetPaginatedEntities<RecommendationChunk>(Arg.Is<GetEntitiesOptions>(opt => opt.Page == 1), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<RecommendationChunk>>(expectedChunks));

            var getRecommendationQuery = new GetRecommendationQuery(repository);

            var result = await getRecommendationQuery.GetChunksByPage(1, CancellationToken.None);

            Assert.Equal(1, result.Pagination.Page);
            Assert.Equal(50, result.Pagination.Total);
            Assert.Equal(expectedChunks.Count, result.Chunks.Count());
        }

        [Fact]
        public async Task GetChunksByPage_CallsGetEntitiesCountOnce()
        {
            var repository = Substitute.For<IContentRepository>();

            repository.GetEntitiesCount<RecommendationChunk>(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(100));

            repository.GetPaginatedEntities<RecommendationChunk>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<RecommendationChunk>>(new List<RecommendationChunk>()));

            var getRecommendationQuery = new GetRecommendationQuery(repository);

            await getRecommendationQuery.GetChunksByPage(1, CancellationToken.None);

            await repository.Received(1).GetEntitiesCount<RecommendationChunk>(Arg.Any<CancellationToken>());
        }
    }
}
