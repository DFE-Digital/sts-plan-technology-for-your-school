using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Persistence.Mappings;

public class WarningComponentMapperTests : BaseMapperTests<WarningComponentDbEntity, WarningComponentMapper>
{
    private static readonly CmsWebHookSystemDetailsInnerContainer WarningComponentText = new() { Sys = new() { Id = "Text Id" } };
    private const string WarningComponentId = "Warning component Id";

    private readonly WarningComponentMapper _mapper;
    private readonly ILogger<WarningComponentMapper> _logger;

    public WarningComponentMapperTests()
    {
        _logger = Substitute.For<ILogger<WarningComponentMapper>>();
        _mapper = new WarningComponentMapper(EntityUpdater, Logger, JsonOptions, DatabaseHelper);
    }

    [Fact]
    public void Mapper_Should_Map_WarningComponent()
    {
        var fields = new Dictionary<string, object?>()
        {
            ["text"] = WrapWithLocalisation(WarningComponentText),
        };

        var payload = CreatePayload(fields, WarningComponentId);

        var mapped = _mapper.ToEntity(payload);

        Assert.NotNull(mapped);

        Assert.Equal(WarningComponentId, mapped.Id);
        Assert.Equal(WarningComponentText.Sys.Id, mapped.TextId);
    }
}
