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

    private static readonly PageDbEntity _loadedPage = new()
    {
        Id = "Page-id",
        Content = []
    };

    private static readonly List<RichTextContentWithSlugDbEntity> _richTextContentsWithSlug =
    [
        new()
        {
            Data = new()
            {
                Uri = "uri"
            },
            Marks = new()
            {
                new()
                {
                    Type = "Bold",
                }
            },
            Content = new(),
            Value = "rich-text",
            Id = 1,
        },

        new()
        {
            Data = new()
            {
                Uri = "uri"
            },
            Marks =
            [
                new()
                {
                    Type = "Bold",
                }
            ],
            Content = [],
            Value = "rich-text",
            Id = 2
        }
    ];

    private readonly List<RichTextContentDbEntity> _returnedRichTextContents = [];

    private readonly ContentfulOptions _contentfulOptions = new(false);

    public GetRichTextsQueryTests()
    {
        _loadedPage.Content.Clear();

        _getRichTextsQuery = new GetRichTextsForPageQuery(_db, _logger, _contentfulOptions);

        _db.RichTextContentWithSlugs.Returns(_richTextContentsWithSlug.AsQueryable());

        _db.ToListCachedAsync(Arg.Any<IQueryable<RichTextContentDbEntity>>(), Arg.Any<CancellationToken>())
            .Returns(callinfo =>
            {
                var queryable = callinfo.ArgAt<IQueryable<RichTextContentDbEntity>>(0);

                return queryable.ToList();
            });

        _db.ToListCachedAsync(Arg.Any<IQueryable<RichTextContentWithSlugDbEntity>>(), Arg.Any<CancellationToken>())
            .Returns(callinfo =>
            {
                var queryable = callinfo.ArgAt<IQueryable<RichTextContentWithSlugDbEntity>>(0);

                return queryable.ToList();
            });
    }

    [Fact]
    public async Task Should_Retrieve_RichTextContents_When_Matching()
    {
        var content = _richTextContentsWithSlug.First();

        _loadedPage.Content.Add(new TextBodyDbEntity()
        {
            RichText = _richTextContentsWithSlug.First(),
            RichTextId = content.Id
        });

        await _getRichTextsQuery.TryLoadChildren(_loadedPage, CancellationToken.None);

        await _db.ReceivedWithAnyArgs(1).ToListCachedAsync(Arg.Any<IQueryable<RichTextContentWithSlugDbEntity>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Retrieve_RichTextContents_When_Not_Matching()
    {
        await _getRichTextsQuery.TryLoadChildren(_loadedPage, CancellationToken.None);

        await _db.ReceivedWithAnyArgs(0)
            .ToListCachedAsync(Arg.Any<IQueryable<RichTextContentWithSlugDbEntity>>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(true, 2)]
    [InlineData(false, 0)]
    public async Task Should_Only_Retrieve_Draft_RichTextContents_When_UsePreviewEnabled(bool usePreview, int expectedContentCount)
    {
        var richTextQuery = new GetRichTextsForPageQuery(_db, _logger, new ContentfulOptions(usePreview));

        var content = _richTextContentsWithSlug.First();
        _loadedPage.Content.Add(new TextBodyDbEntity()
        {
            RichText = content,
            RichTextId = content.Id,
            Published = false
        });

        await richTextQuery.TryLoadChildren(_loadedPage, CancellationToken.None);

        await _db.Received(1)
            .ToListCachedAsync(Arg.Is<IQueryable<RichTextContentWithSlugDbEntity>>(
                arg => arg.Count() == expectedContentCount
                ), Arg.Any<CancellationToken>());
    }
}
