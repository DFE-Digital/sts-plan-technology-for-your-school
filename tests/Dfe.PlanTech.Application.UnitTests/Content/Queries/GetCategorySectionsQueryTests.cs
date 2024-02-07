using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetCategorySectionsQueryTests
{
    private readonly ICmsDbContext _db = Substitute.For<ICmsDbContext>();
    private readonly ILogger<GetCategorySectionsQuery> _logger = Substitute.For<ILogger<GetCategorySectionsQuery>>();

    private readonly GetCategorySectionsQuery _getCategorySectionsQuery;

    private readonly static PageDbEntity _loadedPage = new()
    {
        Id = "Page-id",
        Content = new()
        {

        }
    };

    private readonly static CategoryDbEntity _category = new()
    {
        Id = "category-id",
        ContentPages = new()
        {

        },
        Sections = new()
        {

        }
    };

    private readonly List<SectionDbEntity> _categorySections = new(){
    new(){
      Category = _category,
      CategoryId = _category.Id,
      Id = "section-one",
      Name = "section-one-name",
      Order = 0,
      Questions = new(){
        new(){
          Id = "Question-one",
          Slug = "/question-one",
          HelpText = "helptext-should-be-null",
          Text = "text-should-be-null",
          Order = 0
          },
          new(){
            Id = "Question-two",
            Slug = "/question-two",
            HelpText = "helptext-should-be-null",
            Text = "text-should-be-null",
            Order = 1
            }
          },
          Recommendations = new(){
            new(){
              InternalName = "internalname-should-be-null",
              DisplayName = "Recommendation-one",
              Maturity = Maturity.High,
              Page = new(){
                Slug = "/recommendation-one-slug",
                Id = "recommendation-page-one"
              },
              Id = "recommendation-one"
            },
             new(){
              InternalName = "internalname-should-be-null",
              DisplayName = "Recommendation-two",
              Maturity = Maturity.Medium,
              Page = new(){
                Slug = "/recommendation-two-slug",
                Id = "recommendation-page-two"
              },
              Id = "recommendation-two"
            }
          },
          InterstitialPage = new(){
            Id = "interstitial-page-one",
            Slug = "/page-one",
            InternalName = "should-be-null"
          },
          InterstitialPageId = "interstitial-page-one",
    },
    new(){
      Category = _category,
      CategoryId = _category.Id,
      Id = "section-two",
      Name = "section-two-name",
      Order = 1,
      Questions = new(){
        new(){
          Id = "Question-three",
          Slug = "/question-three",
          HelpText = "helptext-should-be-null",
          Text = "text-should-be-null",
          Order = 0
          },
          new(){
            Id = "Question-four",
            Slug = "/question-four",
            HelpText = "helptext-should-be-null",
            Text = "text-should-be-null",
            Order = 1
            }
          },
          Recommendations = new(){
            new(){
              InternalName = "internalname-should-be-null",
              DisplayName = "Recommendation-three",
              Maturity = Maturity.High,
              Page = new(){
                Slug = "/recommendation-three-slug",
                Id = "recommendation-page-three"
              },
              Id = "recommendation-three"
            },
             new(){
              InternalName = "internalname-should-be-null",
              DisplayName = "Recommendation-four",
              Maturity = Maturity.Medium,
              Page = new(){
                Slug = "/recommendation-four-slug",
                Id = "recommendation-page-four"
              },
              Id = "recommendation-four"
            }
          },
          InterstitialPage = new(){
            Id = "interstitial-page-two",
            Slug = "/page-two",
            InternalName = "should-be-null"
          },
          InterstitialPageId = "interstitial-page-one",

    },
  };

    private readonly List<SectionDbEntity> _sections = new()
  {
    new(){
      Category = new(){

      },
      CategoryId = "other-category"
    }
  };

    public GetCategorySectionsQueryTests()
    {
        _loadedPage.Content.Clear();
        _category.ContentPages.Add(_loadedPage);
        _sections.AddRange(_categorySections);

        _getCategorySectionsQuery = new GetCategorySectionsQuery(_db, _logger);

        _db.Sections.Returns(_sections.AsQueryable());

        _db.ToListAsync(Arg.Any<IQueryable<SectionDbEntity>>(), Arg.Any<CancellationToken>())
            .Returns(callinfo =>
            {
                var queryable = callinfo.ArgAt<IQueryable<SectionDbEntity>>(0);

                return queryable.ToList();
            });
    }

    [Fact]
    public async Task Should_Retrieve_Sections_When_Page_Has_Category()
    {
        _loadedPage.Content.Add(_category);

        await _getCategorySectionsQuery.TryLoadChildren(_loadedPage, CancellationToken.None);

        await _db.ReceivedWithAnyArgs(1)
                     .ToListAsync(Arg.Any<IQueryable<SectionDbEntity>>(), Arg.Any<CancellationToken>());

        var category = _category;

        Assert.Equal(_categorySections.Count, category.Sections.Count);

        foreach (var section in _categorySections)
        {
            var matching = category.Sections.Find(s => section.Id == s.Id);

            Assert.NotNull(matching);

            foreach (var question in section.Questions)
            {
                var matchingQuestion = matching.Questions.Find(q => q.Id == question.Id);
                Assert.NotNull(matchingQuestion);
                Assert.Equal(question.Slug, matchingQuestion.Slug);
                Assert.Null(matchingQuestion.Text);
                Assert.Null(matchingQuestion.HelpText);
            }

            foreach (var recommendation in section.Recommendations)
            {
                var matchingRecommendation = matching.Recommendations.Find(r => r.Id == recommendation.Id);
                Assert.NotNull(matchingRecommendation);
                Assert.Equal(recommendation.DisplayName, matchingRecommendation.DisplayName);
                Assert.Equal(recommendation.Maturity, matchingRecommendation.Maturity);
                Assert.NotNull(matchingRecommendation.DisplayName);
                Assert.Null(matchingRecommendation.InternalName);
            }
        }
    }

    [Fact]
    public async Task Should_Not_Retrieve_ButtonWithEntryReferences_For_Page_When_NoButtons()
    {
        await _getCategorySectionsQuery.TryLoadChildren(_loadedPage, CancellationToken.None);

        await _db.ReceivedWithAnyArgs(0)
                     .ToListAsync(Arg.Any<IQueryable<SectionDbEntity>>(), Arg.Any<CancellationToken>());
    }
}