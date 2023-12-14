using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests;

public class WarningComponentMapperTests : BaseMapperTests
{
  private readonly CmsWebHookSystemDetailsInnerContainer WarningComponentText = new() { Sys = new() { Id = "Text Id" } };
  private const string WarningComponentId = "Warning component Id";

  private readonly WarningComponentMapper _mapper;
  private readonly ILogger<WarningComponentMapper> _logger;

  public WarningComponentMapperTests()
  {
    _logger = Substitute.For<ILogger<WarningComponentMapper>>();
    _mapper = new WarningComponentMapper(_logger, JsonOptions);
  }

  [Fact]
  public void Mapper_Should_Map_WarningComponent()
  {
    var fields = new Dictionary<string, object?>()
    {
      ["text"] = WrapWithLocalisation(WarningComponentText),
    };

    var payload = CreatePayload(fields, WarningComponentId);

    var mapped = _mapper.MapEntity(payload);

    Assert.NotNull(mapped);

    var concrete = mapped as WarningComponentDbEntity;
    Assert.NotNull(concrete);

    Assert.Equal(WarningComponentId, concrete.Id);
    Assert.Equal(WarningComponentText.Sys.Id, concrete.TextId);
  }
}