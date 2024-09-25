using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.UnitTests.Persistence.Mappings;

public class RecommendationSectionMapperTests : BaseMapperTests<RecommendationSectionDbEntity, RecommendationSectionMapper>
{
    private const int InitialOrder = 999;

    private static readonly RecommendationSectionDbEntity ExitingRecommendationSectionDbEntity = new()
    {
        Id = ExistingSectionId,
    };

    private const string ExistingSectionId = "TestingSection";
    private readonly string[] _chunkIds = ["chunk1", "chunk2", "chunk3"];
    private readonly string[] _answerIds = ["answer1", "answer2", "answer3"];

    private readonly List<AnswerDbEntity> _answers;
    private readonly List<RecommendationChunkDbEntity> _chunks;
    private readonly List<RecommendationSectionAnswerDbEntity> _recommendationSectionAnswers = [];
    private readonly List<RecommendationSectionChunkDbEntity> _recommendationSectionChunks = [];
    private readonly List<RecommendationSectionDbEntity> _recommendationSections = [ExitingRecommendationSectionDbEntity];

    private readonly RecommendationSectionMapper _mapper;

    public RecommendationSectionMapperTests()
    {
        _chunks = _chunkIds.Select(id => new RecommendationChunkDbEntity()
        {
            Id = id,
            Order = InitialOrder
        }).ToList();
        _chunks.Add(new RecommendationChunkDbEntity() { Id = "chunk4" });

        _answers = _answerIds.Select(id => new AnswerDbEntity()
        {
            Id = id,
            Order = InitialOrder
        }).ToList();
        _answers.Add(new AnswerDbEntity() { Id = "answer4", Order = InitialOrder });

        _mapper = new RecommendationSectionMapper(EntityUpdater, Logger, JsonOptions, DatabaseHelper);

        MockDatabaseCollection(_answers);
        MockDatabaseCollection(_chunks);
        MockDatabaseCollection(_recommendationSections);
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
        FindLogMessagesContainingStrings(EntityUpdaterLogger, notFoundReferenceIds);
    }

    private CmsWebHookPayload CreateRecommendationSectionPayload(string sectionId, string[] answerIds, string[] chunkIds)
    {
        var answers =
            answerIds.Select(answerId => new CmsWebHookSystemDetailsInnerContainer()
            {
                Sys = new CmsWebHookSystemDetailsInner()
                {
                    Id = answerId
                }
            }).ToArray();

        var chunks =
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
