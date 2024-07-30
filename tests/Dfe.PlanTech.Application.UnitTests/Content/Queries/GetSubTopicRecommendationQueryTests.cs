using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetSubTopicRecommendationQueryTests
{
    private readonly GetSubTopicRecommendationQuery _query;
    private readonly ILogger<GetSubTopicRecommendationQuery> _logger = Substitute.For<ILogger<GetSubTopicRecommendationQuery>>();
    private readonly IGetSubTopicRecommendationQuery _contentfulQuery = Substitute.For<IGetSubTopicRecommendationQuery>();
    private readonly IGetSubTopicRecommendationQuery _dbQuery = Substitute.For<IGetSubTopicRecommendationQuery>();
    private readonly SubtopicRecommendation _subtopicRecommendation;
    private readonly List<SubtopicRecommendation> _subtopicRecommendations = [];

    private readonly List<string> _loggedMessages = [];
    public GetSubTopicRecommendationQueryTests()
    {
        var recommendationSectionOne = new RecommendationSection()
        {
            Answers =
            [
                new(),
                new(),
                new()
            ],
            Chunks =
            [
                new()
                {
                    Answers =
                    [
                        new()
                        {
                            Sys = new SystemDetails() { Id = "1" }
                        },
                        new()
                        {
                            Sys = new SystemDetails() { Id = "2" }
                        },
                        new()
                        {
                            Sys = new SystemDetails() { Id = "3" }
                        }
                    ],

                    Header = new Header()
                    {
                        Tag = HeaderTag.H1,
                        Size = HeaderSize.Large,
                        Text = "chunk1"
                    }

                },
                new()
                {
                    Answers =
                    [
                        new()
                        {
                            Sys = new SystemDetails() { Id = "4" }
                        },
                        new()
                        {
                            Sys = new SystemDetails() { Id = "5" }
                        },
                        new()
                        {
                            Sys = new SystemDetails() { Id = "6" }
                        }
                    ],

                    Header = new Header()
                    {
                        Tag = HeaderTag.H1,
                        Size = HeaderSize.Large,
                        Text = "chunk3"
                    }

                },
                new RecommendationChunk()
                {
                    Answers =
                    [
                        new()
                        {
                            Sys = new() { Id = "7" }
                        },
                        new()
                        {
                            Sys = new() { Id = "8" }
                        },
                        new()
                        {
                            Sys = new() { Id = "9" }
                        }
                    ],

                    Header = new Header()
                    {
                        Tag = HeaderTag.H1,
                        Size = HeaderSize.Large,
                        Text = "chunk2"
                    }
                }
            ]
        };

        var subtopicOne = new Section() { Sys = new() { Id = "SubTopicId" } };

        _subtopicRecommendation = new()
        {
            Intros =
            [
                new() { Maturity = "Low" },
                new() { Maturity = "Medium" },
                new() { Maturity = "High" },
            ],
            Section = recommendationSectionOne,
            Subtopic = subtopicOne,
            Sys = new()
            {
                Id = "Subtopic-Recommendation-Id"
            }
        };

        _subtopicRecommendations.Add(_subtopicRecommendation);
        _query = new GetSubTopicRecommendationQuery(_contentfulQuery, _dbQuery, _logger);
    }

    [Fact]
    public async Task Returns_Result_From_Db_When_Found()
    {
        _dbQuery.GetSubTopicRecommendation(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns((callinfo) =>
                {
                    var subtopicId = _subtopicRecommendation.Subtopic.Sys.Id;

                    return _subtopicRecommendations.FirstOrDefault(recommendation => recommendation.Subtopic.Sys.Id == subtopicId);
                });

        var subtopicRecommendation = await _query.GetSubTopicRecommendation(_subtopicRecommendation.Subtopic.Sys.Id, CancellationToken.None);

        Assert.NotNull(subtopicRecommendation);
        Assert.Equal(subtopicRecommendation.Sys.Id, _subtopicRecommendation!.Sys.Id);
        Assert.Equal(subtopicRecommendation.Subtopic.Sys.Id, _subtopicRecommendation!.Subtopic.Sys.Id);

        var receivedCalls = _logger.ReceivedCalls().ToArray();

        Assert.Single(receivedCalls);

        var callArguments = receivedCalls[0].GetArguments();

        var logLevel = callArguments[0];

        Assert.Equal(LogLevel.Trace, logLevel);

        var formattableStringArguments = callArguments[2] as IReadOnlyList<KeyValuePair<string, object?>>;
        var receivedFrom = formattableStringArguments!.FirstOrDefault(arg => arg.Key == "RetrievedFrom");
        Assert.Equal("database", receivedFrom.Value);
    }

    [Fact]
    public async Task Returns_Result_From_Contentful_When_Db_Not_Found()
    {
        _dbQuery.GetSubTopicRecommendation(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((callinfo) =>
        {
            SubtopicRecommendation? recommendation = null;

            return recommendation;
        });

        _contentfulQuery.GetSubTopicRecommendation(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns((callinfo) =>
                {
                    var subtopicId = _subtopicRecommendation.Subtopic.Sys.Id;

                    return _subtopicRecommendations.FirstOrDefault(recommendation => recommendation.Subtopic.Sys.Id == subtopicId);
                });


        var subtopicRecommendation = await _query.GetSubTopicRecommendation(_subtopicRecommendation.Subtopic.Sys.Id, CancellationToken.None);

        Assert.NotNull(subtopicRecommendation);
        Assert.Equal(subtopicRecommendation.Sys.Id, _subtopicRecommendation!.Sys.Id);
        Assert.Equal(subtopicRecommendation.Subtopic.Sys.Id, _subtopicRecommendation!.Subtopic.Sys.Id);

        var receivedCalls = _logger.ReceivedCalls().ToArray();

        Assert.Single(receivedCalls);
        var callArguments = receivedCalls[0].GetArguments();

        var logLevel = callArguments[0];

        Assert.Equal(LogLevel.Trace, logLevel);

        var formattableStringArguments = callArguments[2] as IReadOnlyList<KeyValuePair<string, object?>>;
        var receivedFrom = formattableStringArguments!.FirstOrDefault(arg => arg.Key == "RetrievedFrom");
        Assert.Equal("Contentful", receivedFrom.Value);
    }

    [Fact]
    public async Task Returns_Null_When_Not_Found_Anywhere()
    {
        _dbQuery.GetSubTopicRecommendation(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(ReturnNullSubtopicRecommendation());

        _contentfulQuery.GetSubTopicRecommendation(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(ReturnNullSubtopicRecommendation());

        var result = await _query.GetSubTopicRecommendation(_subtopicRecommendation.Subtopic.Sys.Id, CancellationToken.None);

        Assert.Null(result);

        var receivedCalls = _logger.ReceivedCalls().ToArray();

        Assert.Single(receivedCalls);
        var callArguments = receivedCalls[0].GetArguments();

        var logLevel = callArguments[0];

        Assert.Equal(LogLevel.Error, logLevel);

        static Func<NSubstitute.Core.CallInfo, SubtopicRecommendation?> ReturnNullSubtopicRecommendation()
        {
            return (callinfo) =>
            {
                SubtopicRecommendation? recommendation = null;

                return recommendation;
            };
        }
    }

}
