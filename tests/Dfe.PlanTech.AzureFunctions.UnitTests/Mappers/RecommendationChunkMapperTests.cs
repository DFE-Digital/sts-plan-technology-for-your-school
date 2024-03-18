using System.Text.Json;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class RecommendationChunkMapperTests : BaseMapperTests
{

    private readonly DbSet<RecommendationChunkAnswerDbEntity> _chunkAnswerDbSet = Substitute.For<DbSet<RecommendationChunkAnswerDbEntity>>();
    private readonly DbSet<RecommendationChunkContentDbEntity> _chunkContentDbSet = Substitute.For<DbSet<RecommendationChunkContentDbEntity>>();

    private static readonly CmsDbContext _db = Substitute.For<CmsDbContext>();
    private static readonly ILogger<RecommendationChunkUpdater> _logger = Substitute.For<ILogger<RecommendationChunkUpdater>>();
    private static RecommendationChunkUpdater CreateMockRecommendationChunkUpdater() => new(_logger, _db);


    [Fact]
    public void RecommendationChunksAreMappedCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["id"] = "ChunkId",
            ["header"] = "HeaderId",
            ["content"] = new string[] { "content1", "content2", "content3" },
            ["answers"] = new string[] { "answer1", "answer2", "answer3" }
        };

        var loggerSubstitute = Substitute.For<ILogger<RecommendationChunkMapper>>();
        var jsonSerializerOptions = new JsonSerializerOptions();

        _db.RecommendationChunkContents = _chunkContentDbSet;
        _db.RecommendationChunkAnswers = _chunkAnswerDbSet;

        var mapper = new RecommendationChunkMapper(MapperHelpers.CreateMockEntityRetriever(),
            CreateMockRecommendationChunkUpdater(), _db, loggerSubstitute, jsonSerializerOptions);

        var recommendationChunk = mapper.PerformAdditionalMapping(values);

        Assert.NotNull(recommendationChunk);

        _db.RecommendationChunkContents.Received(3).Attach(Arg.Any<RecommendationChunkContentDbEntity>());
        _db.RecommendationChunkAnswers.Received(3).Attach(Arg.Any<RecommendationChunkAnswerDbEntity>());
    }
}