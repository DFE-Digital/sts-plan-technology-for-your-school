using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class RecommendationSectionMapperTests : BaseMapperTests
{
    private const string ExistingSectionId = "TestingSection";
    private readonly string[] _chunkIds = ["chunk1", "chunk2", "chunk3"];
    private readonly string[] _answerIds = ["answer1", "answer2", "answer3"];

    private readonly List<AnswerDbEntity> _answers = [];
    private readonly List<RecommendationChunkDbEntity> _chunks = [];
    private readonly List<RecommendationSectionAnswerDbEntity> _recommendationSectionAnswers = [];
    private readonly List<RecommendationSectionChunkDbEntity> _recommendationSectionChunks = [];
    private readonly List<RecommendationSectionDbEntity> _recommendationSections = [];

    private readonly DbSet<AnswerDbEntity> _answersDbSet;
    private readonly DbSet<RecommendationChunkDbEntity> _recommendationChunkDbSet;
    private readonly DbSet<RecommendationSectionAnswerDbEntity> _sectionAnswerDbSet;
    private readonly DbSet<RecommendationSectionChunkDbEntity> _sectionChunkDbSet;
    private readonly DbSet<RecommendationSectionDbEntity> _sectionsDbSet;

    private static readonly CmsDbContext _db = Substitute.For<CmsDbContext>();
    private static readonly ILogger<RecommendationSectionMapper> _recSecMapperLogger = Substitute.For<ILogger<RecommendationSectionMapper>>();
    private readonly ILogger<EntityUpdater> _entityUpdaterLogger;
    private static EntityUpdater CreateMockRecommendationSectionUpdater(ILogger<EntityUpdater> logger) => new(logger, _db);
    private readonly RecommendationSectionMapper _mapper;

    private readonly int InitialOrder = 999;

    private readonly RecommendationSectionDbEntity ExitingRecommendationSectionDbEntity = new()
    {
        Id = ExistingSectionId,
    };

    public RecommendationSectionMapperTests()
    {
        _entityUpdaterLogger = Substitute.For<ILogger<EntityUpdater>>();

        foreach (var id in _chunkIds)
        {
            var chunk = new RecommendationChunkDbEntity()
            {
                Id = id,
                Order = InitialOrder
            };

            _chunks.Add(chunk);
        }
        _chunks.Add(new RecommendationChunkDbEntity() { Id = "chunk4" });

        _recommendationChunkDbSet = _chunks.BuildMockDbSet();
        _db.RecommendationChunks = _recommendationChunkDbSet;

        for (int x = 0; x < _answerIds.Length; x++)
        {
            string id = _answerIds[x];
            var answer = new AnswerDbEntity()
            {
                Id = id,
                Order = InitialOrder
            };

            _answers.Add(answer);
        }
        _answers.Add(new AnswerDbEntity() { Id = "answer4", Order = InitialOrder });

        _answersDbSet = _answers.BuildMockDbSet();
        _db.Answers = _answersDbSet;

        _sectionAnswerDbSet = _recommendationSectionAnswers.BuildMockDbSet();
        _db.RecommendationSectionAnswers = _sectionAnswerDbSet;

        _sectionChunkDbSet = _recommendationSectionChunks.BuildMockDbSet();
        _db.RecommendationSectionChunks = _sectionChunkDbSet;

        _recommendationSections.Add(ExitingRecommendationSectionDbEntity);
        _sectionsDbSet = _recommendationSections.BuildMockDbSet();
        _db.RecommendationSections = _sectionsDbSet;

        _db.Set<AnswerDbEntity>().Returns(_answersDbSet);
        _db.Set<RecommendationChunkDbEntity>().Returns(_recommendationChunkDbSet);
        _db.Set<RecommendationSectionAnswerDbEntity>().Returns(_sectionAnswerDbSet);
        _db.Set<RecommendationSectionChunkDbEntity>().Returns(_sectionChunkDbSet);
        _db.Set<RecommendationSectionDbEntity>().Returns(_sectionsDbSet);

        MockEntityEntry(_db, typeof(RecommendationSectionDbEntity));
        _mapper = new RecommendationSectionMapper(MapperHelpers.CreateMockEntityRetriever(_db), CreateMockRecommendationSectionUpdater(_entityUpdaterLogger), _db, _recSecMapperLogger, JsonOptions);
    }

    [Fact]
    public async Task Should_Create_New_Recommendation_Section_With_Existing_Data()
    {
        var payload = CreateRecommendationSectionPayload("new-section-id", _answerIds, _chunkIds);

        var recommendationSection = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, default);

        Assert.NotNull(recommendationSection);

        var (incoming, existing) = recommendationSection.GetTypedEntities<RecommendationSectionDbEntity>();

        Assert.NotNull(incoming);
        Assert.Null(existing);

        ValidateReferencedContent(_answerIds, incoming.Answers, true, InitialOrder);
        ValidateReferencedContent(_chunkIds, incoming.Chunks, true);
    }

    [Fact]
    public async Task Should_Update_Existing_RecommendationSection()
    {
        string[] answers = [.. _answerIds[1..], "answer4"];
        string[] chunks = [.. _chunkIds[..2], "chunk4"];

        var payload = CreateRecommendationSectionPayload(ExitingRecommendationSectionDbEntity.Id, answers, chunks);

        var recommendationSection = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, default);

        Assert.NotNull(recommendationSection);

        var (incoming, existing) = recommendationSection.GetTypedEntities<RecommendationSectionDbEntity>();

        Assert.NotNull(incoming);
        Assert.NotNull(existing);

        ValidateReferencedContent(answers, existing.Answers, true, InitialOrder);
        ValidateReferencedContent(chunks, existing.Chunks, true);
    }

    [Theory]
    [InlineData("Not-An-Existing-RecSec")]
    [InlineData(ExistingSectionId)]
    public async Task Should_LogWarning_On_Missing_References(string recSecId)
    {
        string[] notFoundAnswers = ["not-existing-answer", "also-not-existing"];
        string[] notFoundChunks = ["not-found-chunk"];
        var payload = CreateRecommendationSectionPayload(recSecId, ["answer1", .. notFoundAnswers], ["chunk3", .. notFoundChunks]);

        var recommendationSection = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, default);

        var (incoming, existing) = recommendationSection.GetTypedEntities<RecommendationSectionDbEntity>();

        Assert.NotNull(incoming);

        var recSecToCheck = (recSecId == ExistingSectionId ? existing : incoming)!;

        if (recSecId == ExistingSectionId)
        {
            Assert.NotNull(existing);
        }
        else
        {
            Assert.Null(existing);
        }

        Assert.Single(recSecToCheck.Answers);
        Assert.Single(recSecToCheck.Chunks);

        string[] notFoundReferenceIds = [.. notFoundAnswers, .. notFoundChunks];
        FindLogMessagesContainingStrings(_entityUpdaterLogger, notFoundReferenceIds);
    }

    private CmsWebHookPayload CreateRecommendationSectionPayload(string sectionId, string[] answerIds, string[] chunkIds)
    {
        CmsWebHookSystemDetailsInnerContainer[] answers =
            answerIds.Select(answerId => new CmsWebHookSystemDetailsInnerContainer()
            {
                Sys = new CmsWebHookSystemDetailsInner()
                {
                    Id = answerId
                }
            }).ToArray();

        CmsWebHookSystemDetailsInnerContainer[] chunks =
            chunkIds.Select(chunkId => new CmsWebHookSystemDetailsInnerContainer()
            {
                Sys = new CmsWebHookSystemDetailsInner()
                {
                    Id = chunkId
                }
            }).ToArray();

        var fields = new Dictionary<string, object?>()
        {
            ["answers"] = WrapWithLocalisation(answers),
            ["chunks"] = WrapWithLocalisation(chunks),
        };

        var payload = CreatePayload(fields, sectionId);
        return payload;
    }
}