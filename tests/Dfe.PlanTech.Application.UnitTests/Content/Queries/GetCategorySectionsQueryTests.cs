using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetCategorySectionsQueryTests
{
    private readonly ICmsDbContext _db = Substitute.For<ICmsDbContext>();
    private readonly ILogger<GetCategorySectionsQuery> _logger = Substitute.For<ILogger<GetCategorySectionsQuery>>();
    private readonly PageDbEntity? _nullPageDbEntity = null;
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

    private readonly static CategoryDbEntity _emptyCategoryId = new()
    {
        Id = "",
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
        }
    }

    [Fact]
    public async Task Should_Not_Retrieve_ButtonWithEntryReferences_For_Page_When_NoButtons()
    {
        await _getCategorySectionsQuery.TryLoadChildren(_loadedPage, CancellationToken.None);

        await _db.ReceivedWithAnyArgs(0)
                     .ToListAsync(Arg.Any<IQueryable<SectionDbEntity>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Log_No_Matching_Category_Error()
    {
        _loadedPage.Content.Add(_emptyCategoryId);
        await _getCategorySectionsQuery.TryLoadChildren(_loadedPage, CancellationToken.None);

        const string errorMessage = "No matching category found.";
        _logger.ReceivedWithAnyArgs(1).Log(LogLevel.Error, Arg.Any<EventId>(), Arg.Any<Exception>(), errorMessage, Arg.Any<object[]>());
        Assert.Equal("", _loadedPage.Content[0].Id);
        Assert.Equal(0, _loadedPage.ContentPages.Count);
    }

    [Fact]
    public async Task Should_Throw_Null_Reference_Exception()
    {
#pragma warning disable CS8604 // Possible null reference argument.
        await Assert.ThrowsAsync<NullReferenceException>(async () => await _getCategorySectionsQuery.TryLoadChildren(_nullPageDbEntity, CancellationToken.None));
#pragma warning restore CS8604 // Possible null reference argument.
    }
}
