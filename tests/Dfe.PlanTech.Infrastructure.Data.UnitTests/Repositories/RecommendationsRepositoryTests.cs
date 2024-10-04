using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data.Repositories;
using Dfe.PlanTech.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Dfe.PlanTech.Infrastructure.Data.UnitTests;

public class RecommendationsRepositoryTests
{
    private const string RecChunkOneId = "recommendation-chunk-one";
    private const string RecIntroOneId = "Intro-One-Low";
#pragma warning disable CA1859 // Use concrete type
    private readonly IRecommendationsRepository _repository;
    private readonly ICmsDbContext _db = Substitute.For<ICmsDbContext>();

    private readonly SubtopicRecommendationDbEntity _subtopicRecommendation;


    private readonly List<AnswerDbEntity> _answers = [];
    private readonly List<RecommendationChunkDbEntity> _chunks = [];
    private readonly List<RecommendationIntroDbEntity> _intros = [];
    private readonly List<SectionDbEntity> _sections = [];
    private readonly List<SubtopicRecommendationDbEntity> _subtopicRecommendations = [];
    private readonly List<RecommendationIntroContentDbEntity> _introContent = [];
    private readonly List<RecommendationChunkContentDbEntity> _chunkContent = [];
    private readonly List<RichTextContentWithSubtopicRecommendationId> _richTexts = [];

    private readonly ILogger<IRecommendationsRepository> _logger = Substitute.For<ILogger<IRecommendationsRepository>>();

    private readonly List<string> LoggedMessages = [];

    public RecommendationsRepositoryTests()
    {
        _subtopicRecommendation = CreateSubtopicRecommendationDbEntity();
        _subtopicRecommendations.Add(_subtopicRecommendation);
        _sections.Add(_subtopicRecommendation.Subtopic);

        foreach (var intro in _subtopicRecommendation.Intros)
        {
            intro.SubtopicRecommendations.Add(_subtopicRecommendation);
        }

        _intros.AddRange(_subtopicRecommendation.Intros);
        _introContent.AddRange(_subtopicRecommendation.Intros.SelectMany((intro, introIndex) => intro.Content
            .Select((content, contentIndex) => new RecommendationIntroContentDbEntity()
            {
                Id = introIndex + contentIndex,
                RecommendationIntro = intro,
                RecommendationIntroId = intro.Id,
                ContentComponent = content,
                ContentComponentId = content.Id
            })));

        foreach (var chunk in _subtopicRecommendation.Section.Chunks)
        {
            chunk.RecommendationSections.Add(_subtopicRecommendation.Section);
        }

        _chunks.AddRange(_subtopicRecommendation.Section.Chunks);
        _chunkContent.AddRange(_subtopicRecommendation.Section.Chunks.SelectMany((chunk, chunkIndex) => chunk.Content
            .Select((content, contentIndex) => new RecommendationChunkContentDbEntity()
            {
                Id = chunkIndex + contentIndex,
                RecommendationChunk = chunk,
                RecommendationChunkId = chunk.Id,
                ContentComponent = content,
                ContentComponentId = content.Id
            })));


        _answers.AddRange([
            .. _subtopicRecommendation.Section.Answers,
            .. _subtopicRecommendation.Section.Chunks.SelectMany(chunk => chunk.Answers)
        ]);

        var subtopicRecDbSetMock = _subtopicRecommendations.BuildMock();
        _db.SubtopicRecommendations.Returns(subtopicRecDbSetMock);
        SetupMockQueryable<SubtopicRecommendationDbEntity>();

        var sectionsDbSetMock = _sections.BuildMock();
        _db.Sections.Returns(sectionsDbSetMock);
        SetupMockQueryable<SectionDbEntity>();

        var introsMockSet = _intros.BuildMock();
        _db.RecommendationIntros.Returns(introsMockSet);
        SetupMockQueryable<RecommendationIntroDbEntity>();

        var chunksMockSet = _chunks.BuildMock();
        _db.RecommendationChunks.Returns(chunksMockSet);
        SetupMockQueryable<RecommendationChunkDbEntity>();

        var introContentMock = _introContent.BuildMock();
        _db.RecommendationIntroContents.Returns(introContentMock);
        SetupMockQueryable<RecommendationIntroContentDbEntity>();

        var chunkContentMock = _chunkContent.BuildMock();
        _db.RecommendationChunkContents.Returns(chunkContentMock);
        SetupMockQueryable<RecommendationChunkContentDbEntity>();

        var richTextsMock = _richTexts.BuildMock();
        _db.RichTextContentWithSubtopicRecommendationIds.Returns(richTextsMock);
        SetupMockQueryable<RichTextContentWithSubtopicRecommendationId>();

        _repository = new RecommendationsRepository(_db, _logger);

        _db.FirstOrDefaultAsync(Arg.Any<TestAsyncEnumerableEfCore<RecommendationsViewDto>>())
            .Returns(args => ((TestAsyncEnumerableEfCore<RecommendationsViewDto>)args[0]).FirstOrDefaultAsync());
    }

    private void SetupMockQueryable<TEntity>()
    {
        _db.ToListAsync(Arg.Any<IQueryable<TEntity>>())
            .Returns(args => ((IQueryable<TEntity>)args[0]).ToListAsync());
        _db.FirstOrDefaultAsync(Arg.Any<IQueryable<TEntity>>())
            .Returns(args => ((IQueryable<TEntity>)args[0]).FirstOrDefaultAsync());
    }

    private static SubtopicRecommendationDbEntity CreateSubtopicRecommendationDbEntity()
    {
        var recommendationSectionOne = new RecommendationSectionDbEntity()
        {
            Answers =
            [
                new AnswerDbEntity()
                {
                    Id = "section-answer-one",
                },
                new AnswerDbEntity()
                {
                    Id = "section-answer-two",
                },
                new AnswerDbEntity()
                {
                    Id = "section-answer-three",
                },
            ],
            Chunks =
            [
                new RecommendationChunkDbEntity()
                {
                    Id = RecChunkOneId,
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
                    Header = "chunk1",
                    Content =
                    [
                        new TextBodyDbEntity()
                        {
                            Id = "Chunk-one-content-one",
                            Order = 0,
                        },
                        new TextBodyDbEntity()
                        {
                            Id = "Chunk-one-content-two",
                            Order = 1,
                        },
                        new TextBodyDbEntity()
                        {
                            Id = "Chunk-one-content-three",
                            Order = 2,
                        },
                    ]
                },
                new RecommendationChunkDbEntity()
                {
                    Id = "recommendation-chunk-two",
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
                    Header = "chunk2",
                    Content =
                    [
                        new TextBodyDbEntity()
                        {
                            Id = "Chunk-two-content-two",
                            Order = 1,
                        },
                        new TextBodyDbEntity()
                        {
                            Id = "Chunk-two-content-thre",
                            Order = 2,
                        },
                        new TextBodyDbEntity()
                        {
                            Id = "Chunk-two-content-one",
                            Order = 0,
                        }
                    ]
                },
                new RecommendationChunkDbEntity()
                {
                    Id = "recommendation-chunk-three",
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
                    Header = "chunk3",
                    Content =
                    [
                        new TextBodyDbEntity()
                        {
                            Id = "Chunk-three-content-three",
                            Order = 2,
                        },
                        new TextBodyDbEntity()
                        {
                            Id = "Chunk-three-content-two",
                            Order = 1,
                        },
                        new TextBodyDbEntity()
                        {
                            Id = "Chunk-three-content-one",
                            Order = 0,
                        }
                    ]
                }
            ],
            Id = "recommendation-section-one"
        };

        var subtopic = new SectionDbEntity() { Id = "SubTopicId" };

        var recommendation = new SubtopicRecommendationDbEntity()
        {
            Intros =
            [
                new RecommendationIntroDbEntity()
                {
                    Maturity = "Low",
                    Id = RecIntroOneId,
                    Slug = "Low-Maturity",
                    Header = new HeaderDbEntity() { Text = "Low maturity header", Id = "Intro-header-one" },
                    Content =
                    [
                        new TextBodyDbEntity()
                        {
                            Id = "Intro-one-content-three",
                            Order = 2,
                        },
                        new TextBodyDbEntity()
                        {
                            Id = "Intro-one-content-two",
                            Order = 1,
                        },
                        new TextBodyDbEntity()
                        {
                            Id = "Intro-one-content-one",
                            Order = 0,
                        }
                    ]
                },
                new RecommendationIntroDbEntity()
                {
                    Maturity = "Medium",
                    Id = "Intro-Two-Medium",
                    Slug = "Medium-Maturity",
                    Header = new HeaderDbEntity() { Text = "Medium maturity header", Id = "Intro-header-two" },
                    Content =
                    [
                        new TextBodyDbEntity()
                        {
                            Id = "Intro-two-content-two",
                            Order = 1,
                        },
                        new TextBodyDbEntity()
                        {
                            Id = "Intro-two-content-three",
                            Order = 2,
                        },
                        new TextBodyDbEntity()
                        {
                            Id = "Intro-two-content-one",
                            Order = 0,
                        }
                    ]
                },
                new RecommendationIntroDbEntity()
                {
                    Maturity = "High",
                    Id = "Intro-Three-High",
                    Slug = "High-Maturity",
                    Header = new HeaderDbEntity() { Text = "High maturity header", Id = "Intro-header-three" },
                    Content =
                    [
                        new TextBodyDbEntity()
                        {
                            Id = "Intro-three-content-three",
                            Order = 2,
                        },
                        new TextBodyDbEntity()
                        {
                            Id = "Intro-three-content-one",
                            Order = 0,
                        },
                        new TextBodyDbEntity()
                        {
                            Id = "Intro-three-content-two",
                            Order = 1,
                        },
                    ]
                },
            ],
            Section = recommendationSectionOne,
            SectionId = recommendationSectionOne.Id,
            Subtopic = subtopic,
            SubtopicId = subtopic.Id,
            Id = "subtopic-recommendation-one"
        };

        subtopic.SubtopicRecommendation = recommendation;

        return recommendation;
    }

    [Theory]
    [InlineData(Maturity.Low, "Low-Maturity", "Low maturity header")]
    [InlineData(Maturity.Medium, "Medium-Maturity", "Medium maturity header")]
    [InlineData(Maturity.High, "High-Maturity", "High maturity header")]
    public async Task GetRecommenationsViewDtoForSubtopicAndMaturity_Should_Retrieve_Partial_Data_When_Found(Maturity maturity, string expectedSlug, string expectedDisplayName)
    {
        var recommendationView = await _repository.GetRecommenationsViewDtoForSubtopicAndMaturity(_subtopicRecommendation.Subtopic.Id, maturity.ToString(), CancellationToken.None);

        Assert.NotNull(recommendationView);
        Assert.Equal(expectedDisplayName, recommendationView.DisplayName);
        Assert.Equal(expectedSlug, recommendationView.RecommendationSlug);
    }

    [Fact]
    public async Task GetRecommenationsViewDtoForSubtopicAndMaturity_Should_Return_Null_When_Not_Found()
    {
        var recommendationView = await _repository.GetRecommenationsViewDtoForSubtopicAndMaturity("not a real subtopic", Maturity.Low.ToString(), CancellationToken.None);
        Assert.Null(recommendationView);
    }

    [Fact]
    public async Task GetRecommenationsViewDtoForSubtopicAndMaturity_Should_Return_Null_When_Maturity_NotFound()
    {
        var recommendationView = await _repository.GetRecommenationsViewDtoForSubtopicAndMaturity(_subtopicRecommendation.Subtopic.Id, "not a real maturity", CancellationToken.None);
        Assert.Null(recommendationView);
    }

    [Fact]
    public async Task GetCompleteRecommendationsForSubtopic_Should_Return_Complete_Subtopic()
    {
        var recommendation = await _repository.GetCompleteRecommendationsForSubtopic(_subtopicRecommendation.Subtopic.Id, CancellationToken.None);

        Validate_GetCompleteRecommendationsForSubtopic_Success(recommendation);
    }

    [Fact]
    public async Task GetCompleteRecommendationsForSubtopic_Should_Return_Null_When_NotFound()
    {
        var recommendation = await _repository.GetCompleteRecommendationsForSubtopic("Not a real subtopic", CancellationToken.None);

        Assert.Null(recommendation);
    }

    [Fact]
    public async Task GetCompleteRecommendationsForSubtopic_Should_Log_InvalidContentRows()
    {
        var recChunk = _subtopicRecommendation.Section.Chunks.FirstOrDefault(chunk => chunk.Id == RecChunkOneId);
        Assert.NotNull(recChunk);

        int[] invalidChunkRowIds = [12345, 123456];
        foreach (var id in invalidChunkRowIds)
        {
            _chunkContent.Add(new()
            {
                Id = id,
                RecommendationChunkId = RecChunkOneId,
                RecommendationChunk = recChunk
            });
        }

        var recIntro = _subtopicRecommendation.Intros.FirstOrDefault(intro => intro.Id == RecIntroOneId);
        Assert.NotNull(recIntro);
        int[] invalidIntroIds = [99999, 342, 1823];
        foreach (var id in invalidIntroIds)
        {
            _introContent.Add(new()
            {
                Id = id,
                RecommendationIntro = recIntro,
                RecommendationIntroId = recIntro.Id
            });
        }

        var recommendation = await _repository.GetCompleteRecommendationsForSubtopic(_subtopicRecommendation.Subtopic.Id, CancellationToken.None);

        Validate_GetCompleteRecommendationsForSubtopic_Success(recommendation);

        var logMessages = _logger.ReceivedCalls().ToArray();
        Assert.Equal(2, logMessages.Length);

        var containsIntroMessage = false;
        var containsChunkMessage = false;
        foreach (var message in logMessages)
        {
            var arguments = message.GetArguments();
            var loggedMessage = arguments[2]?.ToString();
            Assert.NotNull(loggedMessage);

            var idsToCheckFor = loggedMessage.Contains("Chunk") ? invalidChunkRowIds : invalidIntroIds;

            if (loggedMessage.Contains("Chunk"))
            {
                containsChunkMessage = true;
            }
            else
            {
                containsIntroMessage = true;
            }

            foreach (var id in idsToCheckFor)
            {
                Assert.Contains(id.ToString(), loggedMessage);
            }
        }

        Assert.True(containsIntroMessage);
        Assert.True(containsChunkMessage);
    }

    private void Validate_GetCompleteRecommendationsForSubtopic_Success(SubtopicRecommendationDbEntity? recommendation)
    {
        Assert.NotNull(recommendation);
        Assert.Equal(_subtopicRecommendation.Id, recommendation.Id);

        Assert.Equal(_subtopicRecommendation.Intros.Count, recommendation.Intros.Count);

        //Intro content
        for (var x = 0; x < recommendation.Intros.Count; x++)
        {
            var intro = recommendation.Intros[x];
            var matchingIntro = _intros.Find(c => c.Id == intro.Id);

            Assert.NotNull(matchingIntro);

            for (var y = 0; y < intro.Content.Count; y++)
            {
                var content = intro.Content[y];
                var matchingContent = _introContent.Find(ic => ic.ContentComponentId == content.Id);

                Assert.NotNull(matchingContent);
                Assert.NotNull(matchingContent.ContentComponent);

                Assert.Equal(matchingContent.ContentComponent!.Order, y);
            }
        }

        Assert.Equal(_subtopicRecommendation.Section.Chunks.Count, recommendation.Section.Chunks.Count);
        //Chunks
        for (var x = 0; x < recommendation.RecommendationChunk.Count; x++)
        {
            var chunk = recommendation.RecommendationChunk[x];
            var matchingChunk = _chunks.Find(c => c.Id == chunk.Id);

            Assert.NotNull(matchingChunk);

            //Chunk ordering
            Assert.Equal(matchingChunk.Order, x);

            //Chunk content
            for (var y = 0; y < chunk.Content.Count; y++)
            {
                var content = chunk.Content[y];
                var matchingContent = _chunkContent.Find(ic => ic.ContentComponentId == content.Id);

                Assert.NotNull(matchingContent);
                Assert.NotNull(matchingContent.ContentComponent);

                Assert.Equal(matchingContent.ContentComponent!.Order, y);
            }
        }
    }

}
