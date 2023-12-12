using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests;

public class AnswerMapperTests : BaseMapperTests
{
  private readonly AnswerMapper _mapper;
  private readonly ILogger<JsonToDbMapper<AnswerDbEntity>> _logger;
  public AnswerMapperTests()
  {
    _logger = Substitute.For<ILogger<JsonToDbMapper<AnswerDbEntity>>>();
    _mapper = new AnswerMapper(_logger, JsonOptions);
  }

  [Fact]
  public void Mapper_Should_Map_Relationship()
  {
  }
}