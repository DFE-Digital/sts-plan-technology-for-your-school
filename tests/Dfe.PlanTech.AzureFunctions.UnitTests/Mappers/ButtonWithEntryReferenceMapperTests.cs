using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Caching.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class ButtonWithEntryReferenceMapperTests : BaseMapperTests
{
    private const string ButtonWithEntryReferenceId = "ButtonId Id";
    private readonly CmsWebHookSystemDetailsInnerContainer EntryReference = new()
    {
        Sys = new()
        {
            Id = "Entry reference Id"
        }
    };
    private readonly CmsWebHookSystemDetailsInnerContainer ButtonReference = new()
    {
        Sys = new()
        {
            Id = "Button reference Id"
        }
    };

    private readonly ButtonWithEntryReferenceMapper _mapper;
    private readonly ILogger<ButtonWithEntryReferenceMapper> _logger;

    public ButtonWithEntryReferenceMapperTests()
    {
        _logger = Substitute.For<ILogger<ButtonWithEntryReferenceMapper>>();
        _mapper = new ButtonWithEntryReferenceMapper(MapperHelpers.CreateMockEntityRetriever(), MapperHelpers.CreateMockEntityUpdater(), _logger, JsonOptions);
    }

    [Fact]
    public void Mapper_Should_Map_ButtonWithEntryReference()
    {
        var fields = new Dictionary<string, object?>()
        {
            ["button"] = WrapWithLocalisation(ButtonReference),
            ["linkToEntry"] = WrapWithLocalisation(EntryReference),
        };

        var payload = CreatePayload(fields, ButtonWithEntryReferenceId);

        var mapped = _mapper.ToEntity(payload);

        Assert.NotNull(mapped);

        var concrete = mapped;
        Assert.NotNull(concrete);

        Assert.Equal(ButtonWithEntryReferenceId, concrete.Id);
        Assert.Equal(EntryReference.Sys.Id, concrete.LinkToEntryId);
        Assert.Equal(ButtonReference.Sys.Id, concrete.ButtonId);
    }
}
