using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
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
    private readonly ICmsCache _cache = Substitute.For<ICmsCache>();
    private readonly SubtopicRecommendation _subtopicRecommendation;
    private readonly List<SubtopicRecommendation> _subtopicRecommendations = [];

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
                    Header = "chunk1"
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
                    Header = "chunk3",
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
                    Header = "chunk2"
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
        _query = new GetSubTopicRecommendationQuery(_contentfulQuery, _dbQuery, _logger, _cache);
        _cache.GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<SubtopicRecommendation>>>())
            .Returns(callInfo =>
            {
                var func = callInfo.ArgAt<Func<Task<SubtopicRecommendation>>>(1);
                return func();
            });
    }

    [Fact]
    public async Task Returns_Result_From_Contentful()
    {
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

        Assert.Single(_logger.GetMatchingReceivedMessages(
            $"Retrieved subtopic recommendation {_subtopicRecommendation.Subtopic.Sys.Id} from Contentful", LogLevel.Trace));
    }

    [Fact]
    public async Task Returns_Null_When_Not_Found()
    {
        _contentfulQuery.GetSubTopicRecommendation(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(ReturnNullSubtopicRecommendation());

        var result = await _query.GetSubTopicRecommendation(_subtopicRecommendation.Subtopic.Sys.Id, CancellationToken.None);

        Assert.Null(result);

        AssertFailureMessageLog(_subtopicRecommendation);

        static Func<NSubstitute.Core.CallInfo, SubtopicRecommendation?> ReturnNullSubtopicRecommendation()
        {
            return (callinfo) =>
            {
                SubtopicRecommendation? recommendation = null;

                return recommendation;
            };
        }
    }

    private void AssertFailureMessageLog(SubtopicRecommendation? subtopicRecommendation)
    {
        Assert.Single(_logger.GetMatchingReceivedMessages(
            $"Was unable to find a subtopic recommendation for {subtopicRecommendation?.Subtopic.Sys.Id} from DB or Contentful",
            LogLevel.Error));
    }
}
