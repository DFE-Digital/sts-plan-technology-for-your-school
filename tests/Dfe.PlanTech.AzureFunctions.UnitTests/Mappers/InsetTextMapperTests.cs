using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests;

public class InsetTextMapperTests : BaseMapperTests
{
  private const string InsetTextValue = "Inset text goes here";
  private const string InsetTextId = "Header Id";

  private readonly InsetTextMapper _mapper;
  private readonly ILogger<InsetTextMapper> _logger;

  public InsetTextMapperTests()
  {
    _logger = Substitute.For<ILogger<InsetTextMapper>>();
    _mapper = new InsetTextMapper(_logger, JsonOptions);
  }

  [Fact]
  public void Mapper_Should_Map_Relationship()
  {
    var fields = new Dictionary<string, object?>()
    {
      ["text"] = WrapWithLocalisation(InsetTextValue),
    };

    var payload = CreatePayload(fields, InsetTextId);

    var mapped = _mapper.MapEntity(payload);

    Assert.NotNull(mapped);

    var concrete = mapped as InsetTextDbEntity;
    Assert.NotNull(concrete);

    Assert.Equal(InsetTextId, concrete.Id);
    Assert.Equal(InsetTextValue, concrete.Text);
  }
}