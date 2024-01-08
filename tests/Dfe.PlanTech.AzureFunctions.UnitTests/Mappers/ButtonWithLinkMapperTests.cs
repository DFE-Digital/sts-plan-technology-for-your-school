using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests;

public class ButtonWithLinkMapperTests : BaseMapperTests
{
    private const string ButtonWithLinkId = "ButtonId Id";
    private const string Href = "www.website.com";
    private readonly CmsWebHookSystemDetailsInnerContainer ButtonReference = new()
    {
        Sys = new()
        {
            Id = "Button reference Id"
        }
    };

    private readonly ButtonWithLinkMapper _mapper;
    private readonly ILogger<ButtonWithLinkMapper> _logger;

    public ButtonWithLinkMapperTests()
    {
        _logger = Substitute.For<ILogger<ButtonWithLinkMapper>>();
        _mapper = new ButtonWithLinkMapper(_logger, JsonOptions);
    }

    [Fact]
    public void Mapper_Should_Map_ButtonWithEntryReference()
    {
        var fields = new Dictionary<string, object?>()
        {
            ["button"] = WrapWithLocalisation(ButtonReference),
            ["href"] = WrapWithLocalisation(Href),
        };

        var payload = CreatePayload(fields, ButtonWithLinkId);

        var mapped = _mapper.MapEntity(payload);

        Assert.NotNull(mapped);

        var concrete = mapped as ButtonWithLinkDbEntity;
        Assert.NotNull(concrete);

        Assert.Equal(ButtonWithLinkId, concrete.Id);
        Assert.Equal(Href, concrete.Href);
        Assert.Equal(ButtonReference.Sys.Id, concrete.ButtonId);
    }
}