using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetSubTopicRecommendationFromContentfulQueryTests
{
    private readonly IContentRepository _repoSubstitute = Substitute.For<IContentRepository>();
    private readonly GetSubTopicRecommendationFromContentfulQuery _getSubTopicRecommendationFromContentfulQuery;
    private readonly Section _subTopicOne;
    private readonly Section _subTopicTwo;

    private readonly SubTopicRecommendation? _subtopicRecommendationOne;
    private readonly SubTopicRecommendation? _subtopicRecommendationTwo;

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

                    Header = new Header()
                    {
                        Tag = HeaderTag.H1,
                        Size = HeaderSize.Large,
                        Text = "chunk1"
                    }

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

                    Header = new Header()
                    {
                        Tag = HeaderTag.H1,
                        Size = HeaderSize.Large,
                        Text = "chunk3"
                    }

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

                    Header = new Header()
                    {
                        Tag = HeaderTag.H1,
                        Size = HeaderSize.Large,
                        Text = "chunk2"
                    }
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
            Intros = new List<RecommendationIntro>()
            {
                new RecommendationIntro(){ Maturity = "Low"},
                new RecommendationIntro(){ Maturity = "Medium"},
                new RecommendationIntro(){ Maturity = "High"},
            },
            Section = recommendationSectionOne,
            Subtopic = _subTopicOne,
        };

        _subtopicRecommendationTwo = new()
        {
            Subtopic = _subTopicTwo
        };

        _getSubTopicRecommendationFromContentfulQuery = new(_repoSubstitute);
    }

    [Fact]
    public async Task GetSubTopicRecommendation_Returns_Correct_SubTopicRecommendation_From_SectionOne()
    {
        _repoSubstitute.GetEntities<SubTopicRecommendation?>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>()).Returns([_subtopicRecommendationOne]);

        SubTopicRecommendation? subTopicRecommendation = await _getSubTopicRecommendationFromContentfulQuery.GetSubTopicRecommendation(_subTopicOne.Sys.Id);

        Assert.NotNull(subTopicRecommendation);
        Assert.Equal(_subtopicRecommendationOne, subTopicRecommendation);
    }

    [Fact]
    public async Task GetSubTopicRecommendation_Returns_Correct_SubTopicRecommendation_From_SectionTwo()
    {
        _repoSubstitute.GetEntities<SubTopicRecommendation?>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>()).Returns([_subtopicRecommendationTwo]);

        SubTopicRecommendation? subTopicRecommendation = await _getSubTopicRecommendationFromContentfulQuery.GetSubTopicRecommendation(_subTopicTwo.Sys.Id);

        Assert.NotNull(subTopicRecommendation);
        Assert.Equal(_subtopicRecommendationTwo, subTopicRecommendation);
    }

    [Fact]
    public async Task GetSubTopicRecommendationIntro_Returns_Intro_When_Exists_In_SubTopicRecommendation_From_SectionOne()
    {
        _repoSubstitute.GetEntities<SubTopicRecommendation?>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>()).Returns([_subtopicRecommendationOne]);

        SubTopicRecommendation? subTopicRecommendation = await _getSubTopicRecommendationFromContentfulQuery.GetSubTopicRecommendation(_subTopicOne.Sys.Id);

        var intro = subTopicRecommendation!.GetRecommendationByMaturity(Maturity.Medium.ToString());

        Assert.NotNull(intro);
        Assert.Equal(Maturity.Medium.ToString(), intro.Maturity);
    }

    [Fact]
    public async Task GetSubTopicRecommendationChunk_When_List_Of_Answers_Passed_To_RecommendationSection_From_SectionOne()
    {
        _repoSubstitute.GetEntities<SubTopicRecommendation?>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>()).Returns([_subtopicRecommendationOne]);

        SubTopicRecommendation? subTopicRecommendation = await _getSubTopicRecommendationFromContentfulQuery.GetSubTopicRecommendation(_subTopicOne.Sys.Id);

        var chunks = subTopicRecommendation!.Section.GetRecommendationChunksByAnswerIds(new List<string>() { "1", "5", "9" });

        Assert.Equal(3, chunks.Count);
    }

    [Fact]
    public async Task GetSubTopicRecommendationChunk_When_List_Of_Answers_Passed_To_RecommendationSection_From_SectionOne_No_Duplicate_Chunks_Returned()
    {
        _repoSubstitute.GetEntities<SubTopicRecommendation?>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>()).Returns([_subtopicRecommendationOne]);

        SubTopicRecommendation? subTopicRecommendation = await _getSubTopicRecommendationFromContentfulQuery.GetSubTopicRecommendation(_subTopicOne.Sys.Id);

        var chunks = subTopicRecommendation!.Section.GetRecommendationChunksByAnswerIds(["1", "7", "9"]);

        Assert.Equal(2, chunks.Count);
    }

    [Fact]
    public async Task GetSubTopicRecommendationChunks_Returns_Empty_When_List_Of_Answers_Passed_To_RecommendationSection_From_SectionOne_Does_Not_Have_Any_Chunks_Associated()
    {
        _repoSubstitute.GetEntities<SubTopicRecommendation?>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>()).Returns([_subtopicRecommendationOne]);

        SubTopicRecommendation? subTopicRecommendation = await _getSubTopicRecommendationFromContentfulQuery.GetSubTopicRecommendation(_subTopicOne.Sys.Id);

        var chunks = subTopicRecommendation!.Section.GetRecommendationChunksByAnswerIds(["10"]);

        Assert.Empty(chunks);
    }
}