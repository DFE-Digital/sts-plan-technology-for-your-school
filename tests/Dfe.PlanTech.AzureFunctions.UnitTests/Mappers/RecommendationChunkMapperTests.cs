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


    [Fact]
    public void RecommendationChunksAreMappedCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["id"] = "ChunkId",
            ["header"] =  "HeaderId",
            ["content"] = new string[] { "content1", "content2", "content3" },
            ["answers"] = new string[] { "answer1", "answer2", "answer3" }
        };

        var dbSubstitute = Substitute.For<CmsDbContext>();
        var loggerSubstitute = Substitute.For<ILogger<RecommendationChunkMapper>>();
        var jsonSerializerOptions = new JsonSerializerOptions();

        dbSubstitute.RecommendationChunkContents = _chunkContentDbSet;
        dbSubstitute.RecommendationChunkAnswers = _chunkAnswerDbSet;

        var mapper = new RecommendationChunkMapper(MapperHelpers.CreateMockEntityRetriever(),
            MapperHelpers.CreateMockEntityUpdater(), dbSubstitute, loggerSubstitute, jsonSerializerOptions);

        var recommendationChunk = mapper.PerformAdditionalMapping(values);

        Assert.NotNull(recommendationChunk);
        
        dbSubstitute.RecommendationChunkContents.Received(3).Attach(Arg.Any<RecommendationChunkContentDbEntity>());
        dbSubstitute.RecommendationChunkAnswers.Received(3).Attach(Arg.Any<RecommendationChunkAnswerDbEntity>());
    }
}