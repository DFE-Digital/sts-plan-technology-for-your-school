using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetRichTextsQueryTests
{
    private readonly ICmsDbContext _db = Substitute.For<ICmsDbContext>();
    private readonly ILogger<GetRichTextsQuery> _logger = Substitute.For<ILogger<GetRichTextsQuery>>();

    private readonly GetRichTextsQuery _getRichTextsQuery;

    private readonly static PageDbEntity _loadedPage = new()
    {
        Id = "Page-id",
        Content = new()
        {

        }
    };

    private readonly static TextBodyDbEntity _textBody = new()
    {
        Id = "ABCD",
        RichTextId = 1,
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
    }
  };

    private readonly List<RichTextContentDbEntity> _returnedRichTextContents = new();

    public GetRichTextsQueryTests()
    {
        _loadedPage.Content.Clear();

        _getRichTextsQuery = new GetRichTextsQuery(_db, _logger);

        _db.RichTextContentsByPageSlug(Arg.Any<string>()).Returns(_richTextContents.AsQueryable());

        _db.ToListAsync(Arg.Any<IQueryable<RichTextContentDbEntity>>(), Arg.Any<CancellationToken>())
            .Returns(callinfo =>
            {
                var queryable = callinfo.ArgAt<IQueryable<RichTextContentDbEntity>>(0);

                return queryable.ToList();
            });
    }

    [Fact]
    public async Task Should_Retrieve_ButtonWithEntryReferences_For_Page_When_Existing()
    {
        _loadedPage.Content.Add(new TextBodyDbEntity()
        {
            RichText = _richTextContents.First()
        });

        await _getRichTextsQuery.TryLoadChildren(_loadedPage, CancellationToken.None);

        await _db.ReceivedWithAnyArgs(1)
                     .ToListAsync(Arg.Any<IQueryable<RichTextContentDbEntity>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Not_Retrieve_ButtonWithEntryReferences_For_Page_When_NoButtons()
    {
        await _getRichTextsQuery.TryLoadChildren(_loadedPage, CancellationToken.None);

        await _db.ReceivedWithAnyArgs(0)
                     .ToListAsync(Arg.Any<IQueryable<RichTextContentDbEntity>>(), Arg.Any<CancellationToken>());
    }
}