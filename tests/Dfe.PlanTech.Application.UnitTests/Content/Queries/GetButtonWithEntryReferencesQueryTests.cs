using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetButtonWithEntryReferencesQueryTests
{
    private readonly ICmsDbContext _db = Substitute.For<ICmsDbContext>();
    private readonly ILogger<GetButtonWithEntryReferencesQuery> _logger = Substitute.For<ILogger<GetButtonWithEntryReferencesQuery>>();

    private readonly GetButtonWithEntryReferencesQuery _getButtonWithEntryReferencesQuery;

    private readonly static PageDbEntity _pageWithButton = new()
    {
        Id = "Page-id",
        Content = new()
        {

        }
    };


    private readonly static PageDbEntity _pageWithoutButton = new()
    {
        Id = "Page-id",
        Content = new()
        {

        }
    };

    private readonly static PageDbEntity _linkedPage = new()
    {
        Id = "linked-page",
        Slug = "/linked-page-slug"
    };

    private readonly static ButtonWithEntryReferenceDbEntity _button = new()
    {
        Id = "ABCD",
        LinkToEntryId = _linkedPage.Id,
        LinkToEntry = _linkedPage,
        ContentPages = new(){
      _pageWithButton
    }
    };

    private readonly List<ButtonWithEntryReferenceDbEntity> _buttons;

    private readonly List<ButtonWithEntryReferenceDbEntity> _returnedButtons = new();

    public GetButtonWithEntryReferencesQueryTests()
    {
        _getButtonWithEntryReferencesQuery = new GetButtonWithEntryReferencesQuery(_db, _logger);

        _buttons = new()
    {
      _button
    };

        _pageWithButton.Content.Add(_button);
        _db.ButtonWithEntryReferences.Returns(_buttons.AsQueryable());

        _db.ToListAsync(Arg.Any<IQueryable<ButtonWithEntryReferenceDbEntity>>(), Arg.Any<CancellationToken>())
            .Returns(callinfo =>
            {
                var queryable = callinfo.ArgAt<IQueryable<ButtonWithEntryReferenceDbEntity>>(0);

                var result = queryable.ToList();

                _returnedButtons.AddRange(result);

                return result;
            });
    }

    [Fact]
    public async Task Should_Retrieve_ButtonWithEntryReferences_For_Page_When_Existing()
    {
        await _getButtonWithEntryReferencesQuery.TryLoadChildren(_pageWithButton, CancellationToken.None);

        await _db.ReceivedWithAnyArgs(1)
                     .ToListAsync(Arg.Any<IQueryable<ButtonWithEntryReferenceDbEntity>>(), Arg.Any<CancellationToken>());

        Assert.Single(_returnedButtons);

        var returnedButton = _returnedButtons.First();

        var linkToEntry = returnedButton.LinkToEntry as IHasSlug;

        Assert.NotNull(linkToEntry);

        Assert.Equal(_linkedPage.Slug, linkToEntry.Slug);
        Assert.Equal(returnedButton.Id, _button.Id);

        var page = returnedButton.LinkToEntry as PageDbEntity;

        Assert.NotNull(page);
        Assert.Empty(page.Content);
        Assert.Empty(page.BeforeTitleContent);
    }

    [Fact]
    public async Task Should_Not_Retrieve_ButtonWithEntryReferences_For_Page_When_NoButtons()
    {
        await _getButtonWithEntryReferencesQuery.TryLoadChildren(_pageWithoutButton, CancellationToken.None);

        await _db.ReceivedWithAnyArgs(0)
                     .ToListAsync(Arg.Any<IQueryable<ButtonWithEntryReferenceDbEntity>>(), Arg.Any<CancellationToken>());
    }
}
