using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Persistence.Mappings;

public class NavigationLinkMapperTests : BaseMapperTests<NavigationLinkDbEntity, NavigationLinkMapper>
{
    private const string DisplayText = "Nav link text goes here";
    private const string Href = "www.testing.com";
    private const string NavigationLinkId = "Nav link Id";

    private readonly NavigationLinkMapper _mapper;
    private readonly ILogger<NavigationLinkMapper> _logger;

    public NavigationLinkMapperTests()
    {
        _logger = Substitute.For<ILogger<NavigationLinkMapper>>();
        _mapper = new NavigationLinkMapper(EntityUpdater, Logger, JsonOptions, DatabaseHelper);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Mapper_Should_Map_NavigationLink(bool openInNewTab)
    {
        var fields = new Dictionary<string, object?>()
        {
            ["openInNewTab"] = WrapWithLocalisation(openInNewTab),
            ["displayText"] = WrapWithLocalisation(DisplayText),
            ["href"] = WrapWithLocalisation(Href),
        };

        var payload = CreatePayload(fields, NavigationLinkId);

        var mapped = _mapper.ToEntity(payload);

        Assert.NotNull(mapped);

        var concrete = mapped;
        Assert.NotNull(concrete);

        Assert.Equal(NavigationLinkId, concrete.Id);
        Assert.Equal(DisplayText, concrete.DisplayText);
        Assert.Equal(Href, concrete.Href);
        Assert.Equal(openInNewTab, concrete.OpenInNewTab);
    }
}
