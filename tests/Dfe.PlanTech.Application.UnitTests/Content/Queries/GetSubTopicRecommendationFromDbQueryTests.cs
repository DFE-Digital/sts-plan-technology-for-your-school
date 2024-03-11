using AutoMapper;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Mappings;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetSubTopicRecommendationFromDbQueryTests
{
    private readonly ICmsDbContext _db = Substitute.For<ICmsDbContext>();
    private readonly GetSubTopicRecommendationFromDbQuery _query;
    private readonly SectionDbEntity _subTopicOne;
    private readonly SectionDbEntity _subTopicTwo;

    private readonly SubTopicRecommendationDbEntity? _subtopicRecommendationOne;
    private readonly SubTopicRecommendationDbEntity? _subtopicRecommendationTwo;

    private readonly IMapper _mapper;
    private readonly ILogger<GetSubTopicRecommendationFromDbQuery> _logger = Substitute.For<ILogger<GetSubTopicRecommendationFromDbQuery>>();

    private readonly List<SubTopicRecommendationDbEntity> _subtopicRecommendations = [];

    public GetSubTopicRecommendationFromDbQueryTests()
    {
        _mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CmsMappingProfile>();
            cfg.AllowNullCollections = true;
        }).CreateMapper();

        var recommendationSectionOne = new RecommendationSectionDbEntity()
        {
            Answers =
            [
                new AnswerDbEntity(),
                new AnswerDbEntity(),
                new AnswerDbEntity()
            ],
            Chunks =
            [
                new RecommendationChunkDbEntity()
                {
                    Answers =
                    [
                        new AnswerDbEntity()
                        {
                             Id = "1"
                        },
                        new AnswerDbEntity()
                        {
                             Id = "2"
                        },
                        new AnswerDbEntity()
                        {
                             Id = "3"
                        }
                    ],
                    Header = new HeaderDbEntity()
                    {
                        Tag = HeaderTag.H1,
                        Size = HeaderSize.Large,
                        Text = "chunk1",
                        Id = "Header-one"
                    },
                    Content = [
                        new TextBodyDbEntity(){
                            Id = "Chunk-one"
                        }
                    ]
                },
                new RecommendationChunkDbEntity()
                {
                    Answers =
                    [
                        new AnswerDbEntity()
                        {
                            Id = "4"
                        },
                        new AnswerDbEntity()
                        {
                            Id = "5"
                        },
                        new AnswerDbEntity()
                        {
                            Id = "6"
                        }
                    ],
                    Header = new HeaderDbEntity()
                    {
                        Tag = HeaderTag.H1,
                        Size = HeaderSize.Large,
                        Text = "chunk2",
                        Id = "Header-two"
                    },
                    Content = [
                        new TextBodyDbEntity(){
                            Id = "Chunk-two"
                        }
                    ]
                },
                new RecommendationChunkDbEntity()
                {
                    Answers =
                    [
                        new AnswerDbEntity()
                        {
                             Id = "7"
                        },
                        new AnswerDbEntity()
                        {
                           Id = "8"
                        },
                        new AnswerDbEntity()
                        {
                            Id = "9"
                        }
                    ],
                    Header = new HeaderDbEntity()
                    {
                        Tag = HeaderTag.H1,
                        Size = HeaderSize.Large,
                        Text = "chunk3",
                        Id = "Header-three"
                    },
                    Content = [
                        new TextBodyDbEntity(){
                            Id = "Chunk-three"
                        }
                    ]
                }
            ],
            Id = "recommendation-section-one"
        };

        _subTopicOne = new() { Id = "SubTopicId" };

        _subTopicTwo = new() { Id = "IdForAnotherSubTopic" };

        _subtopicRecommendationOne = new()
        {
            Intros =
            [
                new RecommendationIntroDbEntity(){ Maturity = "Low", Id = "Intro-One" },
                new RecommendationIntroDbEntity(){ Maturity = "Medium", Id = "Intro-Two"},
                new RecommendationIntroDbEntity(){ Maturity = "High", Id = "Intro-Three"},
            ],
            Section = recommendationSectionOne,
            SectionId = recommendationSectionOne.Id,
            Subtopic = _subTopicOne,
            SubtopicId = _subTopicOne.Id,
            Id = "subtopic-recommendation-one"
        };

        _subtopicRecommendationTwo = new()
        {
            Subtopic = _subTopicTwo,
            Section = new RecommendationSectionDbEntity()
            {
                Chunks = [],
                Answers = [],
            },
            Intros = [],
            Id = "subtopic-recommendation-two"
        };

        _query = new(_db, _logger, _mapper);

        _subtopicRecommendations.Add(_subtopicRecommendationOne);
        _subtopicRecommendations.Add(_subtopicRecommendationTwo);

        _db.SubtopicRecommendations.Returns(_subtopicRecommendations.AsQueryable());
        _db.FirstOrDefaultAsync(Arg.Any<IQueryable<SubTopicRecommendationDbEntity>>(), Arg.Any<CancellationToken>())
            .Returns(callinfo =>
            {
                var queryable = callinfo.ArgAt<IQueryable<SubTopicRecommendationDbEntity>>(0);

                return queryable.FirstOrDefault();
            });
    }

    [Fact]
    public async Task GetSubTopicRecommendation_Returns_Correct_SubTopicRecommendation_From_SectionOne()
    {
        var subtopicRecommendation = await _query.GetSubTopicRecommendation(_subtopicRecommendationOne!.SubtopicId, CancellationToken.None);

        Assert.NotNull(subtopicRecommendation);
        Assert.Equal(subtopicRecommendation.Sys.Id, _subtopicRecommendationOne!.Id);
        Assert.Equal(subtopicRecommendation.Subtopic.Sys.Id, _subtopicRecommendationOne!.Subtopic.Id);
    }

    [Fact]
    public async Task GetSubTopicRecommendation_Returns_Null_When_Not_Found()
    {
        var subtopicRecommendation = await _query.GetSubTopicRecommendation("not a real id", CancellationToken.None);

        Assert.Null(subtopicRecommendation);
    }

    [Fact]
    public async Task LogsError_When_Exception()
    {
        _db.FirstOrDefaultAsync(Arg.Any<IQueryable<SubTopicRecommendationDbEntity>>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Error"));

        await Assert.ThrowsAnyAsync<Exception>(() => _query.GetSubTopicRecommendation(_subtopicRecommendationOne!.SubtopicId, CancellationToken.None));

        _logger.ReceivedWithAnyArgs(1).Log(default, default, default, default, default!);
    }
}