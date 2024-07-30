using AutoMapper;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Mappings;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Questionnaire.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetSubTopicRecommendationFromDbQueryTests
{
    private readonly IRecommendationsRepository _recommendationsRepository = Substitute.For<IRecommendationsRepository>();
    private readonly GetSubTopicRecommendationFromDbQuery _query;
    private readonly SectionDbEntity _subTopicOne;
    private readonly SectionDbEntity _subTopicTwo;

    private readonly SubtopicRecommendationDbEntity? _subtopicRecommendationOne;
    private readonly SubtopicRecommendationDbEntity? _subtopicRecommendationTwo;
    private readonly SubtopicRecommendationDbEntity? _subtopicRecommendationThree;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSubTopicRecommendationFromDbQuery> _logger = Substitute.For<ILogger<GetSubTopicRecommendationFromDbQuery>>();
    private readonly List<SubtopicRecommendationDbEntity> _subtopicRecommendations = [];


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
                        new TextBodyDbEntity()
                        {
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
                        new TextBodyDbEntity()
                        {
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
                        new TextBodyDbEntity()
                        {
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
                new RecommendationIntroDbEntity() { Maturity = "Low", Id = "Intro-One" },
                new RecommendationIntroDbEntity() { Maturity = "Medium", Id = "Intro-Two" },
                new RecommendationIntroDbEntity() { Maturity = "High", Id = "Intro-Three" },
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

        _subtopicRecommendationThree = new()
        {
            Subtopic = null!,
            SubtopicId = "subtopic-id-three",
            Section = null!,
            Intros = [],
            Id = "subtopic-recommendation-three"
        };


        _query = new(_recommendationsRepository, _logger, _mapper);

        _subtopicRecommendations.Add(_subtopicRecommendationOne);
        _subtopicRecommendations.Add(_subtopicRecommendationTwo);
        _subtopicRecommendations.Add(_subtopicRecommendationThree);

        _recommendationsRepository.GetCompleteRecommendationsForSubtopic(Arg.Any<string>(), Arg.Any<CancellationToken>())
                                  .Returns((callinfo) =>
                                  {
                                      var subtopicId = callinfo.ArgAt<string>(0);

                                      return _subtopicRecommendations.FirstOrDefault(rec => rec.SubtopicId == subtopicId);
                                  });

        _recommendationsRepository.GetRecommenationsViewDtoForSubtopicAndMaturity(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                         .Returns((callinfo) =>
                         {
                             var subtopicId = callinfo.ArgAt<string>(0);
                             var maturity = callinfo.ArgAt<string>(1);

                             return _subtopicRecommendations.Select(subtopicRecommendation => subtopicRecommendation.Intros.FirstOrDefault(intro => intro.Maturity == maturity))
                                                             .Select(intro => intro != null ? new RecommendationsViewDto(intro.Slug, intro.Header.Text) : null)
                                                             .FirstOrDefault();
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
    public async Task LogsError_When_Exception_Retrieving_Recommendations()
    {
        _recommendationsRepository.GetCompleteRecommendationsForSubtopic(Arg.Any<string>(), Arg.Any<CancellationToken>())
                                    .ThrowsAsync((callinfo) => new Exception("Error getting recommendations for subtopic"));

        var exception = await Assert.ThrowsAnyAsync<Exception>(async () => await _query.GetSubTopicRecommendation(_subtopicRecommendationOne!.SubtopicId, CancellationToken.None));

        var loggedMessages = _logger.ReceivedCalls().ToArray();

        Assert.Single(loggedMessages);

        var arguments = loggedMessages.First().GetArguments().ToArray();

        var logLevel = arguments[0];

        Assert.Equal(LogLevel.Error, logLevel);
    }

    [Fact]
    public async Task LogsError_When_Intros_Are_Missing()
    {
        var recommendation = await _query.GetSubTopicRecommendation(_subtopicRecommendationTwo!.SubtopicId, CancellationToken.None);

        Assert.Null(recommendation);

        var loggedMessages = _logger.ReceivedCalls().ToArray();

        Assert.Single(loggedMessages);

        var arguments = loggedMessages.First().GetArguments().ToArray();

        var logLevel = arguments[0];

        Assert.Equal(LogLevel.Error, logLevel);

        var message = GetErrors(arguments);

        Assert.Contains("No intros found", message);
    }

    private static string? GetErrors(object?[] arguments)
    {
        var messages = arguments[2];

        var unboxed = messages as IReadOnlyList<KeyValuePair<string, object?>>;

        var errors = unboxed!.FirstOrDefault(val => val.Key == "Errors");
        var unboxedErrors = errors.Value as string;
        return unboxedErrors;
    }
}
