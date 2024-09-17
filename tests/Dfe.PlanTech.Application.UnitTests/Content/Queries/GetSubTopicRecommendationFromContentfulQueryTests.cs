using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetSubTopicRecommendationFromContentfulQueryTests
{
    private readonly IContentRepository _repoSubstitute = Substitute.For<IContentRepository>();
    private readonly GetSubtopicRecommendationFromContentfulQuery _getSubtopicRecommendationFromContentfulQuery;
    private readonly Section _subTopicOne;
    private readonly Section _subTopicTwo;

    private readonly SubtopicRecommendation? _subtopicRecommendationOne;
    private readonly SubtopicRecommendation? _subtopicRecommendationTwo;

    private readonly List<SubtopicRecommendation> _subtopicRecommendations = [];
    private readonly ILogger<GetSubtopicRecommendationFromContentfulQuery> _logger = Substitute.For<ILogger<GetSubtopicRecommendationFromContentfulQuery>>();

    public GetSubTopicRecommendationFromContentfulQueryTests()
    {
        var recommendationSectionOne = new RecommendationSection()
        {
            Answers = new List<Answer>()
            {
                new Answer(),
                new Answer(),
                new Answer()
            },
            Chunks = new List<RecommendationChunk>()
            {
                new RecommendationChunk()
                {
                    Answers = new List<Answer>()
                    {
                        new Answer()
                        {
                            Sys = new SystemDetails() { Id = "1" }
                        },
                        new Answer()
                        {
                            Sys = new SystemDetails() { Id = "2" }
                        },
                        new Answer()
                        {
                            Sys = new SystemDetails() { Id = "3" }
                        }
                    },
                    Header = "chunk1"

                },
                new RecommendationChunk()
                {
                    Answers = new List<Answer>()
                    {
                        new Answer()
                        {
                            Sys = new SystemDetails() { Id = "4" }
                        },
                        new Answer()
                        {
                            Sys = new SystemDetails() { Id = "5" }
                        },
                        new Answer()
                        {
                            Sys = new SystemDetails() { Id = "6" }
                        }
                    },
                    Header = "chunk3"

                },
                new RecommendationChunk()
                {
                    Answers = new List<Answer>()
                    {
                        new Answer()
                        {
                            Sys = new SystemDetails() { Id = "7" }
                        },
                        new Answer()
                        {
                            Sys = new SystemDetails() { Id = "8" }
                        },
                        new Answer()
                        {
                            Sys = new SystemDetails() { Id = "9" }
                        }
                    },
                    Header = "chunk2"
                }
            }
        };

        _subTopicOne = new()
        {
            Sys = new SystemDetails()
            {
                Id = "SubTopicId"
            }
        };

        _subTopicTwo = new()
        {
            Sys = new SystemDetails()
            {
                Id = "IdForAnotherSubTopic"
            }
        };

        _subtopicRecommendationOne = new()
        {
            Intros =
            [
                new RecommendationIntro() { Maturity = "Low", Header = new Header() { Text = "Low-Maturity-Intro" } },
                new RecommendationIntro() { Maturity = "Medium", Header = new Header() { Text = "Medium-Maturity-Intro" } },
                new RecommendationIntro() { Maturity = "High", Header = new Header() { Text = "High-Maturity-Intro" } },
            ],
            Section = recommendationSectionOne,
            Subtopic = _subTopicOne,
        };

        _subtopicRecommendationTwo = new()
        {
            Subtopic = _subTopicTwo
        };

        _subtopicRecommendations.Add(_subtopicRecommendationOne);
        _subtopicRecommendations.Add(_subtopicRecommendationTwo);

        _getSubtopicRecommendationFromContentfulQuery = new(_repoSubstitute, _logger);
    }

    [Fact]
    public async Task GetSubTopicRecommendation_Returns_Correct_SubTopicRecommendation_From_SectionOne()
    {
        _repoSubstitute.GetEntities<SubtopicRecommendation?>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>()).Returns([_subtopicRecommendationOne]);

        SubtopicRecommendation? subTopicRecommendation = await _getSubtopicRecommendationFromContentfulQuery.GetSubTopicRecommendation(_subTopicOne.Sys.Id);

        Assert.NotNull(subTopicRecommendation);
        Assert.Equal(_subtopicRecommendationOne, subTopicRecommendation);
    }



    [Fact]
    public async Task GetSubTopicRecommendation_Returns_Null_When_Not_Found()
    {
        _repoSubstitute.GetEntities<SubtopicRecommendation?>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>()).Returns([null]);

        SubtopicRecommendation? subTopicRecommendation = await _getSubtopicRecommendationFromContentfulQuery.GetSubTopicRecommendation("not a real id");

        Assert.Null(subTopicRecommendation);
        Assert.Single(_logger.ReceivedCalls());
    }

    [Fact]
    public async Task GetSubTopicRecommendation_Returns_Correct_SubTopicRecommendation_From_SectionTwo()
    {
        _repoSubstitute.GetEntities<SubtopicRecommendation?>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>()).Returns([_subtopicRecommendationTwo]);

        SubtopicRecommendation? subTopicRecommendation = await _getSubtopicRecommendationFromContentfulQuery.GetSubTopicRecommendation(_subTopicTwo.Sys.Id);

        Assert.NotNull(subTopicRecommendation);
        Assert.Equal(_subtopicRecommendationTwo, subTopicRecommendation);
    }

    [Fact]
    public async Task GetSubTopicRecommendationIntro_Returns_Intro_When_Exists_In_SubTopicRecommendation_From_SectionOne()
    {
        _repoSubstitute.GetEntities<SubtopicRecommendation?>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>()).Returns([_subtopicRecommendationOne]);

        SubtopicRecommendation? subTopicRecommendation = await _getSubtopicRecommendationFromContentfulQuery.GetSubTopicRecommendation(_subTopicOne.Sys.Id);

        var intro = subTopicRecommendation!.GetRecommendationByMaturity(Maturity.Medium.ToString());

        Assert.NotNull(intro);
        Assert.Equal(Maturity.Medium.ToString(), intro.Maturity);
    }

    [Fact]
    public async Task GetSubTopicRecommendationChunk_When_List_Of_Answers_Passed_To_RecommendationSection_From_SectionOne()
    {
        _repoSubstitute.GetEntities<SubtopicRecommendation?>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>()).Returns([_subtopicRecommendationOne]);

        SubtopicRecommendation? subTopicRecommendation = await _getSubtopicRecommendationFromContentfulQuery.GetSubTopicRecommendation(_subTopicOne.Sys.Id);

        var chunks = subTopicRecommendation!.Section.GetRecommendationChunksByAnswerIds(new List<string>() { "1", "5", "9" });

        Assert.Equal(3, chunks.Count);
    }

    [Fact]
    public async Task GetSubTopicRecommendationChunk_When_List_Of_Answers_Passed_To_RecommendationSection_From_SectionOne_No_Duplicate_Chunks_Returned()
    {
        _repoSubstitute.GetEntities<SubtopicRecommendation?>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>()).Returns([_subtopicRecommendationOne]);

        SubtopicRecommendation? subTopicRecommendation = await _getSubtopicRecommendationFromContentfulQuery.GetSubTopicRecommendation(_subTopicOne.Sys.Id);

        var chunks = subTopicRecommendation!.Section.GetRecommendationChunksByAnswerIds(["1", "7", "9"]);

        Assert.Equal(2, chunks.Count);
    }

    [Fact]
    public async Task GetSubTopicRecommendationChunks_Returns_Empty_When_List_Of_Answers_Passed_To_RecommendationSection_From_SectionOne_Does_Not_Have_Any_Chunks_Associated()
    {
        _repoSubstitute.GetEntities<SubtopicRecommendation?>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>()).Returns([_subtopicRecommendationOne]);

        SubtopicRecommendation? subTopicRecommendation = await _getSubtopicRecommendationFromContentfulQuery.GetSubTopicRecommendation(_subTopicOne.Sys.Id);

        var chunks = subTopicRecommendation!.Section.GetRecommendationChunksByAnswerIds(["10"]);

        Assert.Empty(chunks);
    }

    [Fact]
    public async Task GetRecommendationsViewDto_Retrieves_Correct_Information_When_Existing()
    {
        _repoSubstitute.GetEntities<SubtopicRecommendation>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>())
                        .Returns((callinfo) =>
                        {
                            var options = callinfo.ArgAt<GetEntitiesOptions>(0);
                            var idFilter = options.Queries!.Where(query => query.Field == "fields.subtopic.sys.id")
                                                            .Select(query => query as ContentQueryEquals)
                                                            .Select(query => query!.Value).FirstOrDefault();

                            return _subtopicRecommendations.Where(rec => rec.Subtopic.Sys.Id == idFilter)
                                                            .Select(rec => new SubtopicRecommendation()
                                                            {
                                                                Intros = rec.Intros,
                                                                Sys = rec.Sys
                                                            });
                        });

        var maturity = "Low";
        var expectedIntro = _subtopicRecommendationOne!.Intros.FirstOrDefault(intro => intro.Maturity == maturity);

        Assert.NotNull(expectedIntro);

        var result = await _getSubtopicRecommendationFromContentfulQuery.GetRecommendationsViewDto(_subtopicRecommendationOne!.Subtopic.Sys.Id, maturity);

        Assert.NotNull(result);
        Assert.Equal(result.RecommendationSlug, expectedIntro.Slug);
        Assert.Equal(result.DisplayName, expectedIntro.HeaderText);
    }

    [Fact]
    public async Task GetRecommendationsViewDto_Returns_Null_When_No_Match()
    {
        _repoSubstitute.GetEntities<SubtopicRecommendation>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>())
                        .Returns((callinfo) => []);

        var result = await _getSubtopicRecommendationFromContentfulQuery.GetRecommendationsViewDto("any id", "Low");

        Assert.Null(result);

        Assert.Single(_logger.ReceivedCalls());
    }
}
