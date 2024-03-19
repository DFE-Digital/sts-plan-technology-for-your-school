using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data.Repositories;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Dfe.PlanTech.Infrastructure.Data.UnitTests;

public class RecommendationsRepositoryTests
{
#pragma warning disable CA1859 // Use concrete type
  private readonly IRecommendationsRepository _repository;
#pragma warning disable CA1859 // Use concrete type

  private readonly ICmsDbContext _db = Substitute.For<ICmsDbContext>();

  private readonly SubtopicRecommendationDbEntity _subtopicRecommendation;


  private readonly List<AnswerDbEntity> _answers = [];
  private readonly List<RecommendationChunkDbEntity> _chunks = [];
  private readonly List<RecommendationIntroDbEntity> _intros = [];
  private readonly List<SectionDbEntity> _sections = [];
  private readonly List<SubtopicRecommendationDbEntity> _subtopicRecommendations = [];

  public RecommendationsRepositoryTests()
  {
    _subtopicRecommendation = CreateSubtopicRecommendationDbEntity();
    _subtopicRecommendations.Add(_subtopicRecommendation);

    _answers.AddRange([.. _subtopicRecommendation.Section.Answers, .. _subtopicRecommendation.Section.Chunks.SelectMany(chunk => chunk.Answers)]);
    _chunks.AddRange(_subtopicRecommendation.Section.Chunks);
    _intros.AddRange(_subtopicRecommendation.Section.RecommendationIntro);
    _sections.Add(_subtopicRecommendation.Subtopic);

    var testing = new List<SubtopicRecommendationDbEntity>() { new() { Id = "One" } };
    var queryable = testing.AsQueryable();

    var mockContext = Substitute.For<ICmsDbContext>();

    var subtopicRecDbSetMock = _subtopicRecommendations.BuildMock();
    _db.SubtopicRecommendations.Returns(subtopicRecDbSetMock);

    var sectionsDbSetMock = _sections.BuildMock();
    _db.Sections.Returns(sectionsDbSetMock);

    var introsMockSet = _intros.BuildMock();
    _db.RecommendationIntros.Returns(introsMockSet);

    var chunksMockSet = _chunks.BuildMock();
    _db.RecommendationChunks.Returns(chunksMockSet);

    List<RecommendationIntroContentDbEntity> introContent = [];
    var introContentMock = introContent.BuildMock();
    _db.RecommendationIntroContents.Returns(introContentMock);

    List<RecommendationChunkContentDbEntity> chunkContent = [];
    var chunkContentMock = chunkContent.BuildMock();
    _db.RecommendationChunkContents.Returns(chunkContentMock);


    List<RichTextContentWithSubtopicRecommendationId> richTexts = [];
    var richTextsMock = richTexts.BuildMock();
    _db.RichTextContentWithSubtopicRecommendationIds.Returns(richTextsMock);

    _repository = new RecommendationsRepository(_db);
  }

  private SubtopicRecommendationDbEntity CreateSubtopicRecommendationDbEntity()
  {
    var recommendationSectionOne = new RecommendationSectionDbEntity()
    {
      Answers = [
        new AnswerDbEntity()
        {
          Id= "section-answer-one",
        },
        new AnswerDbEntity(){
          Id= "section-answer-two",
        },
        new AnswerDbEntity(){
          Id= "section-answer-three",
        },
      ],
      Chunks =
          [
          new RecommendationChunkDbEntity()
            {
              Id = "recommendation-chunk-one",
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

    var subtopic = new SectionDbEntity() { Id = "SubTopicId" };

    var recommendation = new SubtopicRecommendationDbEntity()
    {
      Intros =
        [
          new RecommendationIntroDbEntity(){ Maturity = "Low", Id = "Intro-One-Low", Slug = "Low-Maturity", Header = new HeaderDbEntity() { Text = "Low maturity header", Id = "Intro-header-one"} },
          new RecommendationIntroDbEntity(){ Maturity = "Medium", Id = "Intro-Two-Medium", Slug = "Medium-Maturity", Header = new HeaderDbEntity() { Text = "Medium maturity header", Id = "Intro-header-two"} },
          new RecommendationIntroDbEntity(){ Maturity = "High", Id = "Intro-Three-High", Slug = "High-Maturity", Header = new HeaderDbEntity() { Text = "High maturity header", Id = "Intro-header-three"} },
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

    Assert.NotNull(recommendation);
    Assert.Equal(_subtopicRecommendation.Id, recommendation.Id);
  }

  [Fact]
  public async Task GetCompleteRecommendationsForSubtopic_Should_Return_Null_When_NotFound()
  {
    var recommendation = await _repository.GetCompleteRecommendationsForSubtopic("Not a real subtopic", CancellationToken.None);

    Assert.Null(recommendation);
  }
}
