using System.Text.Json;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class RecommendationChunkMapperTests : BaseMapperTests
{
    private const string ExistingChunkId = "TestingChunk";
    private readonly string[] _contentIds = ["content1", "content2", "content3", "content4", "content5"];
    private readonly string[] _answerIds = ["answer1", "answer2", "answer3", "answer4", "answer5"];

    private readonly List<AnswerDbEntity> _answers = [];
    private readonly List<ContentComponentDbEntity> _contentComponents = [];
    private readonly List<RecommendationChunkDbEntity> _chunks = [];
    private readonly List<RecommendationChunkAnswerDbEntity> _recommendationChunkAnswers = [];
    private readonly List<RecommendationChunkContentDbEntity> _recommendationChunkContents = [];

    private readonly DbSet<AnswerDbEntity> _answersDbSet;
    private readonly DbSet<ContentComponentDbEntity> _contentComponentsDbSet;
    private readonly DbSet<RecommendationChunkDbEntity> _recommendationChunkDbSet;
    private readonly DbSet<RecommendationChunkAnswerDbEntity> _chunkAnswerDbSet;
    private readonly DbSet<RecommendationChunkContentDbEntity> _chunkContentDbSet;

    private static readonly CmsDbContext _db = Substitute.For<CmsDbContext>();
    private static readonly ILogger<RecommendationChunkMapper> _recSecMapperLogger = Substitute.For<ILogger<RecommendationChunkMapper>>();
    private readonly ILogger<EntityUpdater> _entityUpdaterLogger;
    private static EntityUpdater CreateMockRecommendationChunkUpdater(ILogger<EntityUpdater> logger) => new(logger, _db);
    private readonly RecommendationChunkMapper _mapper;

    private readonly RecommendationChunkDbEntity ExistingRecommendationChunk = new()
    {
        Id = ExistingChunkId,
    };

    public RecommendationChunkMapperTests()
    {
        _entityUpdaterLogger = Substitute.For<ILogger<EntityUpdater>>();
        _chunks.Add(ExistingRecommendationChunk);

        for (var x = 0; x < _contentIds.Length; x++)
        {
            var contentId = _contentIds[x];
            var content = new ContentComponentDbEntity()
            {
                Id = contentId,
                Order = x
            };

            _contentComponents.Add(content);

            if (x < 3)
            {
                var chunkContent = new RecommendationChunkContentDbEntity()
                {
                    Id = x,
                    RecommendationChunk = ExistingRecommendationChunk,
                    RecommendationChunkId = ExistingChunkId,
                    ContentComponent = content,
                    ContentComponentId = content.Id,
                };

                _recommendationChunkContents.Add(chunkContent);
            }
        }

        _contentComponentsDbSet = _contentComponents.BuildMockDbSet();
        _db.ContentComponents = _contentComponentsDbSet;

        _recommendationChunkDbSet = _chunks.BuildMockDbSet();
        _db.RecommendationChunks = _recommendationChunkDbSet;

        for (int x = 0; x < _answerIds.Length; x++)
        {
            string id = _answerIds[x];
            var answer = new AnswerDbEntity()
            {
                Id = id,
                Order = x
            };

            _answers.Add(answer);
        }

        _answers.Add(new AnswerDbEntity() { Id = "answer4" });

        _answersDbSet = _answers.BuildMockDbSet();
        _db.Answers = _answersDbSet;

        _chunkAnswerDbSet = _recommendationChunkAnswers.BuildMockDbSet();
        _db.RecommendationChunkAnswers = _chunkAnswerDbSet;

        _chunkContentDbSet = _recommendationChunkContents.BuildMockDbSet();
        _db.RecommendationChunkContents = _chunkContentDbSet;

        _chunks.Add(ExistingRecommendationChunk);
        _recommendationChunkDbSet = _chunks.BuildMockDbSet();
        _db.RecommendationChunks = _recommendationChunkDbSet;

        _db.Set<AnswerDbEntity>().Returns(_answersDbSet);
        _db.Set<ContentComponentDbEntity>().Returns(_contentComponentsDbSet);
        _db.Set<RecommendationChunkDbEntity>().Returns(_recommendationChunkDbSet);
        _db.Set<RecommendationChunkAnswerDbEntity>().Returns(_chunkAnswerDbSet);
        _db.Set<RecommendationChunkContentDbEntity>().Returns(_chunkContentDbSet);

        MockEntityEntry(_db, typeof(RecommendationChunkDbEntity), typeof(AnswerDbEntity), typeof(ContentComponentDbEntity), typeof(RecommendationChunkAnswerDbEntity), typeof(RecommendationChunkContentDbEntity));
        _mapper = new RecommendationChunkMapper(MapperHelpers.CreateMockEntityRetriever(_db), CreateMockRecommendationChunkUpdater(_entityUpdaterLogger), _db, _recSecMapperLogger, JsonOptions);
    }

    [Fact]
    public async Task Should_Create_New_Recommendation_Chunk_With_Existing_Data()
    {
        var payload = CreateRecommendationChunkPayload("new-chunk-id", _answerIds, _contentIds);

        var RecommendationChunk = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, default);

        Assert.NotNull(RecommendationChunk);

        var (incoming, existing) = RecommendationChunk.GetTypedEntities<RecommendationChunkDbEntity>();

        Assert.NotNull(incoming);
        Assert.Null(existing);

        Assert.Equal(_answerIds.Length, incoming.Answers.Count);

        foreach (var answerId in _answerIds)
        {
            var matching = incoming.Answers.FirstOrDefault(answer => answer.Id == answerId);
            Assert.NotNull(matching);
        }

        Assert.Equal(_contentIds.Length, incoming.Content.Count);
        foreach (var contentId in _contentIds)
        {
            var matching = incoming.Content.FirstOrDefault(answer => answer.Id == contentId);
            Assert.NotNull(matching);
        }
    }

    [Fact]
    public async Task Should_Update_Existing_Chunk()
    {
        string[] answers = [.. _answerIds[1..]];
        string[] contents = [.. _contentIds[2..]];
        var payload = CreateRecommendationChunkPayload(ExistingRecommendationChunk.Id, answers, contents);

        var RecommendationChunk = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, default);

        Assert.NotNull(RecommendationChunk);

        var (incoming, existing) = RecommendationChunk.GetTypedEntities<RecommendationChunkDbEntity>();

        Assert.NotNull(incoming);
        Assert.NotNull(existing);

        Assert.Equal(answers.Length, existing.Answers.Count);

        foreach (var answerId in answers)
        {
            var matching = existing.Answers.FirstOrDefault(answer => answer.Id == answerId);
            Assert.NotNull(matching);
        }

        Assert.Equal(contents.Length, existing.Content.Count);
        foreach (var contentId in contents)
        {
            var matching = existing.Content.FirstOrDefault(answer => answer.Id == contentId);
            Assert.NotNull(matching);
        }
    }

    [Theory]
    [InlineData("Not-An-Existing-RecSec")]
    [InlineData(ExistingChunkId)]
    public async Task Should_LogWarning_On_Missing_References(string recSecId)
    {
        string[] notFoundAnswers = ["not-existing-answer", "also-not-existing"];
        string[] notFoundContent = ["not-found-content"];
        var payload = CreateRecommendationChunkPayload(recSecId, ["answer1", .. notFoundAnswers], ["content3", .. notFoundContent]);

        var RecommendationChunk = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, default);

        var (incoming, existing) = RecommendationChunk.GetTypedEntities<RecommendationChunkDbEntity>();

        Assert.NotNull(incoming);

        var recSecToCheck = (recSecId == ExistingChunkId ? existing : incoming)!;

        if (recSecId == ExistingChunkId)
        {
            Assert.NotNull(existing);
        }
        else
        {
            Assert.Null(existing);
        }

        Assert.Single(recSecToCheck.Answers);
        Assert.Single(recSecToCheck.Content);

        string[] notFoundReferenceIds = [.. notFoundAnswers, .. notFoundContent];
        FindLogMessagesContainingStrings(_entityUpdaterLogger, notFoundReferenceIds);
    }

    private CmsWebHookPayload CreateRecommendationChunkPayload(string chunkId, string[] answerIds, string[] contentIds)
    {
        CmsWebHookSystemDetailsInnerContainer[] answers =
            answerIds.Select(answerId => new CmsWebHookSystemDetailsInnerContainer()
            {
                Sys = new CmsWebHookSystemDetailsInner()
                {
                    Id = answerId
                }
            }).ToArray();

        CmsWebHookSystemDetailsInnerContainer[] contents =
            contentIds.Select(chunkId => new CmsWebHookSystemDetailsInnerContainer()
            {
                Sys = new CmsWebHookSystemDetailsInner()
                {
                    Id = chunkId
                }
            }).ToArray();

        var fields = new Dictionary<string, object?>()
        {
            ["answers"] = WrapWithLocalisation(answers),
            ["content"] = WrapWithLocalisation(contents),
        };

        var payload = CreatePayload(fields, chunkId);
        return payload;
    }
}