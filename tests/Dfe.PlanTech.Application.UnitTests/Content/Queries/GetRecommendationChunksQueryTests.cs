using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetRecommendationChunksQueryTests
{
    private readonly IGetSubTopicRecommendation _getSubTopicRecommendationSubstitute = Substitute.For<IGetSubTopicRecommendation>();
    private readonly GetRecommendationChunksQuery _getRecommendationChunksQuery;

    private readonly Answer _answerWithLinkOne;
    private readonly Answer _answerWithLinkTwo;
    private readonly Answer _answerWithLinkThree;
    private readonly Answer _answerWithLinkFour;
    private readonly Answer _answerWithLinkFive;

    private readonly Answer _answerWithNoLinkToChunk;

    private readonly Answer _specialAnswerWithLinkToRecommendationSectionOne;
    private readonly Answer _specialAnswerWithLinkToRecommendationSectionTwo;

    private readonly TextBody _chunkContentOne;
    private readonly TextBody _chunkContentTwo;
    private readonly TextBody _chunkContentThree;

    private readonly RecommendationChunk _recommendationChunkOne;
    private readonly RecommendationChunk _recommendationChunkTwo;
    private readonly RecommendationChunk _recommendationChunkThree;
    private readonly RecommendationChunk _recommendationChunkFour;

    private readonly Section _subTopic;

    public GetRecommendationChunksQueryTests()
    {
        _answerWithLinkOne = new()
        {
            Sys = new SystemDetails()
            {
                Id = "answerWithLinkOne"
            }
        };

        _answerWithLinkTwo = new()
        {
            Sys = new SystemDetails()
            {
                Id = "answerWithLinkTwo"
            }
        };

        _answerWithLinkThree = new()
        {
            Sys = new SystemDetails()
            {
                Id = "answerWithLinkThree"
            }
        };

        _answerWithLinkFour = new()
        {
            Sys = new SystemDetails()
            {
                Id = "answerWithLinkFour"
            }
        };

        _answerWithLinkFive = new()
        {
            Sys = new SystemDetails()
            {
                Id = "answerWithLinkFive"
            }
        };

        _answerWithNoLinkToChunk = new()
        {
            Sys = new SystemDetails()
            {
                Id = "answerWithNoLinkToChunk"
            }
        };

        _specialAnswerWithLinkToRecommendationSectionOne = new()
        {
            Sys = new SystemDetails()
            {
                Id = "specialAnswerWithLinkToRecommendationSectionOne"
            }
        };

        _specialAnswerWithLinkToRecommendationSectionTwo = new()
        {
            Sys = new SystemDetails()
            {
                Id = "specialAnswerWithLinkToRecommendationSectionTwo"
            }
        };

        _chunkContentOne = new()
        {
            RichText = new()
            {
                Value = "Text Body One"
            }
        };

        _chunkContentTwo = new()
        {
            RichText = new()
            {
                Value = "Text Body Two"
            }
        };

        _chunkContentThree = new()
        {
            RichText = new()
            {
                Value = "Text Body Three"
            }
        };

        _recommendationChunkOne = new()
        {
            Title = "Chunk One",
            Header = new() { Text = "Header One" },
            Content = [_chunkContentOne],
            Answers = [_answerWithLinkOne]
        };

        _recommendationChunkTwo = new()
        {
            Title = "Chunk Two",
            Header = new() { Text = "Header Two" },
            Content = [_chunkContentTwo],
            Answers = [_answerWithLinkTwo]
        };

        _recommendationChunkThree = new()
        {
            Title = "Chunk Three",
            Header = new() { Text = "Header Three" },
            Content = [_chunkContentThree],
            Answers = [_answerWithLinkThree]
        };

        _recommendationChunkFour = new()
        {
            Title = "Chunk Four",
            Header = new() { Text = "Header Four" },
            Content = [],
            Answers = [_answerWithLinkFour, _answerWithLinkFive]
        };

        RecommendationSection recommendationSection = new()
        {
            Answers = [_specialAnswerWithLinkToRecommendationSectionOne, _specialAnswerWithLinkToRecommendationSectionTwo],
            Chunks = [_recommendationChunkOne, _recommendationChunkTwo, _recommendationChunkThree, _recommendationChunkFour]
        };

        SubTopicRecommendation subTopicRecommendation = new()
        {
            Section = recommendationSection
        };

        _subTopic = new()
        {
            Sys = new SystemDetails()
            {
                Id = "SubTopicId"
            }
        };

        _getSubTopicRecommendationSubstitute.GetSubTopicRecommendation(_subTopic).Returns(subTopicRecommendation);

        _getRecommendationChunksQuery = new(_getSubTopicRecommendationSubstitute);
    }

    [Fact]
    public async Task GetRecommendationChunksFromAnswers_Returns_AllChunks_If_AnyAnswer_IsIn_RecommendationSection()
    {
        IEnumerable<Answer> answers = [_specialAnswerWithLinkToRecommendationSectionOne, _specialAnswerWithLinkToRecommendationSectionTwo];

        List<RecommendationChunk>? recommendationChunks = (await _getRecommendationChunksQuery.GetRecommendationChunksFromAnswers(_subTopic, answers)).ToList();

        Assert.NotNull(recommendationChunks);
        Assert.NotEmpty(recommendationChunks);
        Assert.Equal(4, recommendationChunks.Count);

        Assert.Equal(_recommendationChunkOne, recommendationChunks[0]);
        Assert.Equal(_recommendationChunkTwo, recommendationChunks[1]);
        Assert.Equal(_recommendationChunkThree, recommendationChunks[2]);
        Assert.Equal(_recommendationChunkFour, recommendationChunks[3]);
    }

    [Fact]
    public async Task GetRecommendationChunksFromAnswers_Returns_ChunkOne_From_AnswerOne()
    {
        IEnumerable<Answer> answers = [_answerWithLinkOne];

        List<RecommendationChunk>? recommendationChunks = (await _getRecommendationChunksQuery.GetRecommendationChunksFromAnswers(_subTopic, answers)).ToList();

        Assert.NotNull(recommendationChunks);
        Assert.NotEmpty(recommendationChunks);
        Assert.Single(recommendationChunks);

        Assert.Equal(_recommendationChunkOne, recommendationChunks[0]);

        Assert.Equal("Chunk One", recommendationChunks[0].Title);
        Assert.Equal("Header One", recommendationChunks[0].Header.Text);
        Assert.Equal(_chunkContentOne, recommendationChunks[0].Content[0]);
        Assert.Equal("answerWithLinkOne", recommendationChunks[0].Answers[0].Sys.Id);
    }

    [Fact]
    public async Task GetRecommendationChunksFromAnswers_Returns_ChunkTwo_From_AnswerTwo()
    {
        IEnumerable<Answer> answers = [_answerWithLinkTwo];

        List<RecommendationChunk>? recommendationChunks = (await _getRecommendationChunksQuery.GetRecommendationChunksFromAnswers(_subTopic, answers)).ToList();

        Assert.NotNull(recommendationChunks);
        Assert.NotEmpty(recommendationChunks);
        Assert.Single(recommendationChunks);

        Assert.Equal(_recommendationChunkTwo, recommendationChunks[0]);

        Assert.Equal("Chunk Two", recommendationChunks[0].Title);
        Assert.Equal("Header Two", recommendationChunks[0].Header.Text);
        Assert.Equal(_chunkContentTwo, recommendationChunks[0].Content[0]);
        Assert.Equal("answerWithLinkTwo", recommendationChunks[0].Answers[0].Sys.Id);
    }

    [Fact]
    public async Task GetRecommendationChunksFromAnswers_Returns_ChunkThree_From_AnswerThree()
    {
        IEnumerable<Answer> answers = [_answerWithLinkThree];

        List<RecommendationChunk>? recommendationChunks = (await _getRecommendationChunksQuery.GetRecommendationChunksFromAnswers(_subTopic, answers)).ToList();

        Assert.NotNull(recommendationChunks);
        Assert.NotEmpty(recommendationChunks);
        Assert.Single(recommendationChunks);

        Assert.Equal(_recommendationChunkThree, recommendationChunks[0]);

        Assert.Equal("Chunk Three", recommendationChunks[0].Title);
        Assert.Equal("Header Three", recommendationChunks[0].Header.Text);
        Assert.Equal(_chunkContentThree, recommendationChunks[0].Content[0]);
        Assert.Equal("answerWithLinkThree", recommendationChunks[0].Answers[0].Sys.Id);
    }

    [Fact]
    public async Task GetRecommendationChunksFromAnswers_Returns_CorrectChunks_From_MultipleAnswers()
    {
        IEnumerable<Answer> answers = [_answerWithLinkOne, _answerWithLinkThree];

        List<RecommendationChunk>? recommendationChunks = (await _getRecommendationChunksQuery.GetRecommendationChunksFromAnswers(_subTopic, answers)).ToList();

        Assert.NotNull(recommendationChunks);
        Assert.NotEmpty(recommendationChunks);
        Assert.Equal(2, recommendationChunks.Count);

        Assert.Equal(_recommendationChunkOne, recommendationChunks[0]);
        Assert.Equal(_recommendationChunkThree, recommendationChunks[1]);

        Assert.Equal("Chunk One", recommendationChunks[0].Title);
        Assert.Equal("Header One", recommendationChunks[0].Header.Text);
        Assert.Equal(_chunkContentOne, recommendationChunks[0].Content[0]);
        Assert.Equal("answerWithLinkOne", recommendationChunks[0].Answers[0].Sys.Id);

        Assert.Equal("Chunk Three", recommendationChunks[1].Title);
        Assert.Equal("Header Three", recommendationChunks[1].Header.Text);
        Assert.Equal(_chunkContentThree, recommendationChunks[1].Content[0]);
        Assert.Equal("answerWithLinkThree", recommendationChunks[1].Answers[0].Sys.Id);
    }

    [Fact]
    public async Task GetRecommendationChunksFromAnswers_Returns_CorrectChunk_If_ItHas_MultipleAnswers_With_AnswerFour()
    {
        IEnumerable<Answer> answers = [_answerWithLinkFour];

        List<RecommendationChunk>? recommendationChunks = (await _getRecommendationChunksQuery.GetRecommendationChunksFromAnswers(_subTopic, answers)).ToList();

        Assert.NotNull(recommendationChunks);
        Assert.NotEmpty(recommendationChunks);
        Assert.Single(recommendationChunks);

        Assert.Equal(_recommendationChunkFour, recommendationChunks[0]);

        Assert.Equal("Chunk Four", recommendationChunks[0].Title);
        Assert.Equal("Header Four", recommendationChunks[0].Header.Text);
        Assert.Equal("answerWithLinkFour", recommendationChunks[0].Answers[0].Sys.Id);
        Assert.Equal("answerWithLinkFive", recommendationChunks[0].Answers[1].Sys.Id);
    }

    [Fact]
    public async Task GetRecommendationChunksFromAnswers_Returns_CorrectChunk_If_ItHas_MultipleAnswers_With_AnswerFive()
    {
        IEnumerable<Answer> answers = [_answerWithLinkFive];

        List<RecommendationChunk>? recommendationChunks = (await _getRecommendationChunksQuery.GetRecommendationChunksFromAnswers(_subTopic, answers)).ToList();

        Assert.NotNull(recommendationChunks);
        Assert.NotEmpty(recommendationChunks);
        Assert.Single(recommendationChunks);

        Assert.Equal(_recommendationChunkFour, recommendationChunks[0]);

        Assert.Equal(2, recommendationChunks[0].Answers.Count);

        Assert.Equal("Chunk Four", recommendationChunks[0].Title);
        Assert.Equal("Header Four", recommendationChunks[0].Header.Text);
        Assert.Equal("answerWithLinkFour", recommendationChunks[0].Answers[0].Sys.Id);
        Assert.Equal("answerWithLinkFive", recommendationChunks[0].Answers[1].Sys.Id);
    }

    [Fact]
    public async Task GetRecommendationChunksFromAnswers_Returns_CorrectChunks_From_MultipleAnswers_With_DifferentAnswer_Counts()
    {
        IEnumerable<Answer> answers = [_answerWithLinkOne, _answerWithLinkFive];

        List<RecommendationChunk>? recommendationChunks = (await _getRecommendationChunksQuery.GetRecommendationChunksFromAnswers(_subTopic, answers)).ToList();

        Assert.NotNull(recommendationChunks);
        Assert.NotEmpty(recommendationChunks);
        Assert.Equal(2, recommendationChunks.Count);

        Assert.Equal(_recommendationChunkOne, recommendationChunks[0]);
        Assert.Equal(_recommendationChunkFour, recommendationChunks[1]);

        Assert.Equal("Chunk One", recommendationChunks[0].Title);
        Assert.Equal("Header One", recommendationChunks[0].Header.Text);
        Assert.Equal(_chunkContentOne, recommendationChunks[0].Content[0]);
        Assert.Equal("answerWithLinkOne", recommendationChunks[0].Answers[0].Sys.Id);

        Assert.Equal(2, recommendationChunks[1].Answers.Count);

        Assert.Equal("Chunk Four", recommendationChunks[1].Title);
        Assert.Equal("Header Four", recommendationChunks[1].Header.Text);
        Assert.Equal("answerWithLinkFour", recommendationChunks[1].Answers[0].Sys.Id);
        Assert.Equal("answerWithLinkFive", recommendationChunks[1].Answers[1].Sys.Id);
    }

    [Fact]
    public async Task GetRecommendationChunksFromAnswers_Removes_Duplicates_FromReturn()
    {
        IEnumerable<Answer> answers = [_answerWithLinkFour, _answerWithLinkFive];

        List<RecommendationChunk>? recommendationChunks = (await _getRecommendationChunksQuery.GetRecommendationChunksFromAnswers(_subTopic, answers)).ToList();

        Assert.NotNull(recommendationChunks);
        Assert.NotEmpty(recommendationChunks);
        Assert.Single(recommendationChunks);

        Assert.Equal(_recommendationChunkFour, recommendationChunks[0]);

        Assert.Equal(2, recommendationChunks[0].Answers.Count);

        Assert.Equal("Chunk Four", recommendationChunks[0].Title);
        Assert.Equal("Header Four", recommendationChunks[0].Header.Text);
        Assert.Equal("answerWithLinkFour", recommendationChunks[0].Answers[0].Sys.Id);
        Assert.Equal("answerWithLinkFive", recommendationChunks[0].Answers[1].Sys.Id);
    }

    [Fact]
    public async Task GetRecommendationChunksFromAnswers_Returns_Nothing_If_AnswerIsNotReferenced_ByAnyChunk()
    {
        IEnumerable<Answer> answers = [_answerWithNoLinkToChunk];

        List<RecommendationChunk>? recommendationChunks = (await _getRecommendationChunksQuery.GetRecommendationChunksFromAnswers(_subTopic, answers)).ToList();

        Assert.NotNull(recommendationChunks);
        Assert.Empty(recommendationChunks);
    }
}