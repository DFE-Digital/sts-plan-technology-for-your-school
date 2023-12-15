using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests;

public class RecommendationPageMapperTests : BaseMapperTests
{
  private const string RecommendationId = "Recommendation Id 1234";
  private const string DisplayName = "Recommendation page display name";
  private const string InternalName = "Recommendation page internal name";
  private const Maturity RecommendationMaturity = Maturity.Medium;
  private readonly CmsWebHookSystemDetailsInnerContainer Page = new()
  {
    Sys = new()
    {
      Id = "Page Reference Id"
    }
  };

  private readonly RecommendationPageMapper _mapper;
  private readonly ILogger<RecommendationPageMapper> _logger;

  public RecommendationPageMapperTests()
  {
    _logger = Substitute.For<ILogger<RecommendationPageMapper>>();
    _mapper = new RecommendationPageMapper(_logger, JsonOptions);
  }

  [Fact]
  public void Mapper_Should_Map_Relationship()
  {
    var fields = new Dictionary<string, object?>()
    {
      ["displayName"] = WrapWithLocalisation(DisplayName),
      ["internalName"] = WrapWithLocalisation(InternalName),
      ["maturity"] = WrapWithLocalisation(RecommendationMaturity),
      ["page"] = WrapWithLocalisation(Page),
    };

    var payload = CreatePayload(fields, RecommendationId);

    var mapped = _mapper.MapEntity(payload);

    Assert.NotNull(mapped);

    var concrete = mapped as RecommendationPageDbEntity;
    Assert.NotNull(concrete);

    Assert.Equal(RecommendationId, concrete.Id);
    Assert.Equal(DisplayName, concrete.DisplayName);
    Assert.Equal(InternalName, concrete.InternalName);
    Assert.Equal(Page.Sys.Id, concrete.PageId);
  }
}