using AutoMapper;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Mappings;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

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

    public GetSubTopicRecommendationFromDbQueryTests()
    {
        _mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CmsMappingProfile>();
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
                        Text = "chunk1"
                    }

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
                        Text = "chunk3"
                    }
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
                        Text = "chunk2"
                    }
                }
            ]
        };

        _subTopicOne = new() { Id = "SubTopicId" };

        _subTopicTwo = new() { Id = "IdForAnotherSubTopic" };

        _subtopicRecommendationOne = new()
        {
            Intros =
            [
                new RecommendationIntroDbEntity(){ Maturity = "Low"},
                new RecommendationIntroDbEntity(){ Maturity = "Medium"},
                new RecommendationIntroDbEntity(){ Maturity = "High"},
            ],
            Section = recommendationSectionOne,
            Subtopic = _subTopicOne,
        };

        _subtopicRecommendationTwo = new()
        {
            Subtopic = _subTopicTwo
        };

        _query = new(_db, _logger, _mapper);
    }

    [Fact]
    public async Task GetSubTopicRecommendation_Returns_Correct_SubTopicRecommendation_From_SectionOne()
    {
    }

    [Fact]
    public async Task GetSubTopicRecommendation_Returns_Correct_SubTopicRecommendation_From_SectionTwo()
    {
    }

    [Fact]
    public async Task GetSubTopicRecommendationIntro_Returns_Intro_When_Exists_In_SubTopicRecommendation_From_SectionOne()
    {
    }

    [Fact]
    public async Task GetSubTopicRecommendationChunk_When_List_Of_Answers_Passed_To_RecommendationSection_From_SectionOne()
    {
    }

    [Fact]
    public async Task GetSubTopicRecommendationChunk_When_List_Of_Answers_Passed_To_RecommendationSection_From_SectionOne_No_Duplicate_Chunks_Returned()
    {
    }

    [Fact]
    public async Task GetSubTopicRecommendationChunks_Returns_Empty_When_List_Of_Answers_Passed_To_RecommendationSection_From_SectionOne_Does_Not_Have_Any_Chunks_Associated()
    {
    }
}