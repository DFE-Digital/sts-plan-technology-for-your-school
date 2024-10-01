using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.UnitTests.Persistence.Mappings;

public class RecommendationChunkMapperTests : BaseMapperTests<RecommendationChunkDbEntity, RecommendationChunkMapper>
{
    private const string ExistingChunkId = "TestingChunk";
    private readonly string[] _contentIds = ["content1", "content2", "content3", "content4", "content5"];
    private readonly string[] _answerIds = ["answer1", "answer2", "answer3", "answer4", "answer5"];

    private static readonly RecommendationChunkDbEntity ExistingRecommendationChunk = new()
    {
        Id = ExistingChunkId,
    };

    private readonly List<AnswerDbEntity> _answers = [];
    private readonly List<ContentComponentDbEntity> _contentComponents = [];
    private readonly List<RecommendationChunkDbEntity> _chunks = [ExistingRecommendationChunk];
    private readonly List<RecommendationChunkAnswerDbEntity> _recommendationChunkAnswers = [];
    private readonly List<RecommendationChunkContentDbEntity> _recommendationChunkContents = [];
    private readonly RecommendationChunkMapper _mapper;

    private readonly int _initialOrder = 999;

    public RecommendationChunkMapperTests()
    {
        for (var x = 0; x < _contentIds.Length; x++)
        {
            var contentId = _contentIds[x];
            var content = new ContentComponentDbEntity()
            {
                Id = contentId,
                Order = _initialOrder,
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

        for (int x = 0; x < _answerIds.Length; x++)
        {
            string id = _answerIds[x];
            var answer = new AnswerDbEntity()
            {
                Id = id,
                Order = _initialOrder
            };

            _answers.Add(answer);
        }

        _answers.Add(new AnswerDbEntity() { Id = "answer4" });
        _mapper = new RecommendationChunkMapper(EntityUpdater, Logger, JsonOptions, DatabaseHelper);

        MockDatabaseCollection(_answers);
        MockDatabaseCollection(_contentComponents);
        MockDatabaseCollection(_recommendationChunkAnswers);
        MockDatabaseCollection(_recommendationChunkContents);
        MockDatabaseCollection(_chunks);
    }

    [Fact]
    public async Task Should_Create_New_Recommendation_Chunk_With_Existing_Data()
    {
        var payload = CreateRecommendationChunkPayload("new-chunk-id", _answerIds, _contentIds, null);

        var recommendationChunk = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, default);

        Assert.NotNull(recommendationChunk);

        var (incoming, existing) = recommendationChunk.GetTypedEntities<RecommendationChunkDbEntity>();

        Assert.NotNull(incoming);
        Assert.Null(existing);

        Assert.Equal(_answerIds.Length, incoming.Answers.Count);

        ValidateReferencedContent(_answerIds, incoming.Answers, true, _initialOrder);
        ValidateReferencedContent(_contentIds, incoming.Content, true);
    }

    [Fact]
    public async Task Should_Update_Existing_Chunk()
    {
        string[] answers = [.. _answerIds[1..]];
        string[] contents = [.. _contentIds[2..]];
        string csLinkId = "csLink-id";

        var payload = CreateRecommendationChunkPayload(
            ExistingRecommendationChunk.Id,
            answers,
            contents,
            csLinkId
        );

        var RecommendationChunk = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, default);

        Assert.NotNull(RecommendationChunk);

        var (incoming, existing) = RecommendationChunk.GetTypedEntities<RecommendationChunkDbEntity>();

        Assert.NotNull(incoming);
        Assert.NotNull(existing);
        Assert.Equal(csLinkId, incoming.CSLinkId);

        ValidateReferencedContent(answers, existing.Answers, true, _initialOrder);
        ValidateReferencedContent(contents, existing.Content, true);
    }

    [Theory]
    [InlineData("Not-An-Existing-RecSec")]
    [InlineData(ExistingChunkId)]
    public async Task Should_LogWarning_On_Missing_References(string recSecId)
    {
        string[] notFoundAnswers = ["not-existing-answer", "also-not-existing"];
        string[] notFoundContent = ["not-found-content"];
        var payload = CreateRecommendationChunkPayload(
            recSecId,
            ["answer1", .. notFoundAnswers],
            ["content3", .. notFoundContent],
            null
        );

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
        FindLogMessagesContainingStrings(EntityUpdaterLogger, notFoundReferenceIds);
    }

    private void ValidateAnswers(string[] answerIds, RecommendationChunkDbEntity recommendationChunk)
    {
        Assert.Equal(answerIds.Length, recommendationChunk.Answers.Count);

        for (int x = 0; x < answerIds.Length; x++)
        {
            var answerId = answerIds[x];
            var matching = recommendationChunk.Answers.FirstOrDefault(answer => answer.Id == answerId);
            Assert.NotNull(matching);

            //The order should NOT have changed
            Assert.Equal(_initialOrder, matching.Order);
        }
    }

    private static void ValidateContent(string[] contentIds, RecommendationChunkDbEntity existing)
    {
        Assert.Equal(contentIds.Length, existing.Content.Count);

        for (int x = 0; x < contentIds.Length; x++)
        {
            var contentId = contentIds[x];
            var matching = existing.Content.FirstOrDefault(answer => answer.Id == contentId);
            Assert.NotNull(matching);
            Assert.Equal(x, matching.Order);
        }
    }

    private CmsWebHookPayload CreateRecommendationChunkPayload(
        string chunkId,
        string[] answerIds,
        string[] contentIds,
        string? csLinkId
    )
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

        if (csLinkId != null)
        {
            var link = new CmsWebHookSystemDetailsInnerContainer()
            {
                Sys = new CmsWebHookSystemDetailsInner()
                {
                    Id = csLinkId
                }
            };
            fields["csLinkId"] = WrapWithLocalisation(link);
        }

        var payload = CreatePayload(fields, chunkId);
        return payload;
    }
}
