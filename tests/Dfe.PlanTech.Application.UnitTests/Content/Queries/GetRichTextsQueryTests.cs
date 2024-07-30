using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetRichTextsQueryTests
{
    private readonly ICmsDbContext _db = Substitute.For<ICmsDbContext>();
    private readonly ILogger<GetRichTextsForPageQuery> _logger = Substitute.For<ILogger<GetRichTextsForPageQuery>>();

    private readonly GetRichTextsForPageQuery _getRichTextsQuery;

    private readonly static PageDbEntity _loadedPage = new()
    {
        Id = "Page-id",
        Content = new()
        {

        }
    };

    private readonly static List<RichTextContentDbEntity> _richTextContents = new(){
    new()
    {
      Data = new()
      {
        Uri = "uri"
      },
      Marks = new(){
        new(){
          Type = "Bold",
        }
      },
      Content = new()
      {

      },
      Value = "rich-text",
      Id = 1,
          },new()
    {
      Data = new()
      {
        Uri = "uri"
      },
      Marks = [
        new(){
          Type = "Bold",
        }
      ],
      Content = [],
      Value = "rich-text",
      Id = 2
          }
  };

    private readonly List<RichTextContentDbEntity> _returnedRichTextContents = new();

    private readonly ContentfulOptions _contentfulOptions = new(false);

    public GetRichTextsQueryTests()
    {
        _loadedPage.Content.Clear();

        _getRichTextsQuery = new GetRichTextsForPageQuery(_db, _logger, _contentfulOptions);

        _db.RichTextContents.Returns(_richTextContents.AsQueryable());

        _db.ToListAsync(Arg.Any<IQueryable<RichTextContentDbEntity>>(), Arg.Any<CancellationToken>())
            .Returns(callinfo =>
            {
                var queryable = callinfo.ArgAt<IQueryable<RichTextContentDbEntity>>(0);

                return queryable.ToList();
            });
    }

    [Fact]
    public async Task Should_Retrieve_RichTextContents_When_Matching()
    {
        _loadedPage.Content.Add(new TextBodyDbEntity()
        {
            RichText = _richTextContents.First()
        });

        await _getRichTextsQuery.TryLoadChildren(_loadedPage, CancellationToken.None);

        await _db.ReceivedWithAnyArgs(1)
                     .ToListAsync(Arg.Any<IQueryable<RichTextContentWithSlugDbEntity>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Retrieve_RichTextContents_When_Not_Matching()
    {
        await _getRichTextsQuery.TryLoadChildren(_loadedPage, CancellationToken.None);

        await _db.ReceivedWithAnyArgs(0)
                     .ToListAsync(Arg.Any<IQueryable<RichTextContentWithSlugDbEntity>>(), Arg.Any<CancellationToken>());
    }
}
