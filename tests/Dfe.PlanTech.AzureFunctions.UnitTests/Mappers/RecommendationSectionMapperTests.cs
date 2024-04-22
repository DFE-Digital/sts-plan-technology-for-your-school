using System.Text.Json;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Extensions;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class RecommendationSectionMapperTests : BaseMapperTests
{
    private readonly DbSet<RecommendationSectionAnswerDbEntity> _sectionAnswerDbSet = Substitute.For<DbSet<RecommendationSectionAnswerDbEntity>>();
    private readonly DbSet<RecommendationSectionChunkDbEntity> _sectionChunkDbSet = Substitute.For<DbSet<RecommendationSectionChunkDbEntity>>();
    private static readonly CmsDbContext _db = Substitute.For<CmsDbContext>();
    private static readonly ILogger<RecommendationSectionMapper> _logger = Substitute.For<ILogger<RecommendationSectionMapper>>();
    private static RecommendationSectionUpdater CreateMockRecommendationSectionUpdater() => new(Substitute.For<ILogger<RecommendationSectionUpdater>>(), _db);
    private readonly List<RecommendationChunkDbEntity> _chunks = [];
    private readonly JsonSerializerOptions _jsonSerializerOptions = new();
    private readonly string[] _chunkIds = ["chunk1", "chunk2", "chunk3"];
    private readonly RecommendationSectionMapper _mapper;

    public RecommendationSectionMapperTests()
    {
        foreach (var id in _chunkIds)
        {
            var chunk = new RecommendationChunkDbEntity()
            {
                Id = id,
                Order = 99999
            };

            _chunks.Add(chunk);
        }

        IQueryable<RecommendationChunkDbEntity> queryable = _chunks.AsQueryable();

        var asyncProvider = new AsyncQueryProvider<RecommendationChunkDbEntity>(queryable.Provider);

        var mockPageDataSet = Substitute.For<DbSet<RecommendationChunkDbEntity>, IQueryable<RecommendationChunkDbEntity>>();
        ((IQueryable<RecommendationChunkDbEntity>)mockPageDataSet).Provider.Returns(asyncProvider);
        ((IQueryable<RecommendationChunkDbEntity>)mockPageDataSet).Expression.Returns(queryable.Expression);
        ((IQueryable<RecommendationChunkDbEntity>)mockPageDataSet).ElementType.Returns(queryable.ElementType);
        ((IQueryable<RecommendationChunkDbEntity>)mockPageDataSet).GetEnumerator().Returns(queryable.GetEnumerator());

        _db.RecommendationChunks = mockPageDataSet;


        _db.RecommendationSectionChunks = _sectionChunkDbSet;
        _db.RecommendationSectionAnswers = _sectionAnswerDbSet;

        _db.RecommendationSectionChunks = _sectionChunkDbSet;
        _db.RecommendationSectionAnswers = _sectionAnswerDbSet;

        _mapper = new RecommendationSectionMapper(MapperHelpers.CreateMockEntityRetriever(), CreateMockRecommendationSectionUpdater(), _db, _logger, _jsonSerializerOptions);
    }

    [Fact]
    public void RecommendationSectionsAreMappedCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["id"] = "sectionId",
            ["chunks"] = _chunkIds,
            ["answers"] = new string[] { "answer1", "answer2", "answer3" }
        };

        var recommendationChunk = _mapper.PerformAdditionalMapping(values);

        Assert.NotNull(recommendationChunk);

        _db.RecommendationSectionAnswers.Received(3).Attach(Arg.Any<RecommendationSectionAnswerDbEntity>());
        _db.RecommendationSectionChunks.Received(3).Attach(Arg.Any<RecommendationSectionChunkDbEntity>());

        for (var x = 0; x < _chunkIds.Length; x++)
        {
            var matchingChunk = _chunks.FirstOrDefault(c => c.Id == _chunkIds[x]);
            Assert.NotNull(matchingChunk);
            Assert.Equal(x, matchingChunk?.Order);
        }
    }

    [Fact]
    public void Should_LogWarning_On_Missing_Chunks()
    {
        var missingChunkId = "not a real chunk id";
        string[] chunkIds = [.. _chunkIds, missingChunkId];
        var values = new Dictionary<string, object?>
        {
            ["id"] = "sectionId",
            ["chunks"] = chunkIds,
            ["answers"] = new string[] { "answer1", "answer2", "answer3" }
        };


        var recommendationChunk = _mapper.PerformAdditionalMapping(values);

        Assert.NotNull(recommendationChunk);

        var receivedLogWarnings = _logger.ReceivedCalls();

        Assert.Single(receivedLogWarnings);

        var call = receivedLogWarnings.First();

        var arguments = call.GetArguments();

        Assert.NotNull(arguments);
        var errorMessage = arguments[2]!.ToString();

        Assert.Contains(missingChunkId, errorMessage);

        var logLevel = Enum.Parse<LogLevel>(arguments[0]!.ToString()!);

        Assert.Equal(LogLevel.Warning, logLevel);
    }
}