using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class JsonToDbMapperUnitTests
{
  private const string StringValueTest = "string test";
  private readonly string[] ArrayValueTest = new[] { "one", "two", "three" };
  private const int NumberValueTest = 10000;
  private const string EntityId = "Testing Id";
  private readonly CmsWebHookSystemDetailsInnerContainer[] ReferencesValueTest = new[] {
        new CmsWebHookSystemDetailsInnerContainer(){
          Sys = new(){
            Id = "First reference"
          }
        }, new CmsWebHookSystemDetailsInnerContainer(){
          Sys = new(){
            Id = "Second reference"
          }
        }
      };

  private readonly JsonToDbMapperImplementation _mapper;

  private readonly ILogger _logger;
  private readonly JsonSerializerOptions _jsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
  };

  private readonly Type _type = typeof(ContentComponentDbEntityImplementation);

  public JsonToDbMapperUnitTests()
  {
    _logger = Substitute.For<ILogger>();
    _mapper = new JsonToDbMapperImplementation(_type, _logger, _jsonOptions);
  }

  [Fact]
  public void AcceptsContentType_Should_Return_True_When_Matched()
  {
    var acceptsContentType = _mapper.AcceptsContentType(_type.Name);

    Assert.True(acceptsContentType);
  }

  [Fact]
  public void AcceptsContentType_Should_Return_False_When_Not_Matched()
  {
    var acceptsContentType = _mapper.AcceptsContentType("Different content type");

    Assert.False(acceptsContentType);
  }

  [Fact]
  public void Should_Flatten_Payload_And_Return_ContentComponentDbEntity_When_Valid_Payload()
  {
    var fields = new Dictionary<string, object?>()
    {
      ["stringValue"] = WrapWithLocalisation(StringValueTest),
      ["numberValue"] = WrapWithLocalisation(NumberValueTest),
      ["arrayValue"] = WrapWithLocalisation(ArrayValueTest),
      ["referenceIds"] = WrapWithLocalisation(ReferencesValueTest)
    };

    var asJson = JsonSerializer.Serialize(fields, _jsonOptions);
    var asJsonNode = JsonSerializer.Deserialize<Dictionary<string, JsonNode>>(asJson, _jsonOptions);

    var payload = new CmsWebHookPayload()
    {
      Sys = new CmsWebHookSystemDetails()
      {
        Id = EntityId
      },
      Fields = asJsonNode!
    };


    var mapped = _mapper.MapEntity(payload);

    Assert.NotNull(mapped);

    var concrete = mapped as ContentComponentDbEntityImplementation;
    Assert.NotNull(concrete);

    Assert.Equal(EntityId, concrete.Id);

    Assert.Equal(StringValueTest, concrete.StringValue);
    Assert.Equal(NumberValueTest, concrete.NumberValue);
    Assert.Equal(ArrayValueTest, concrete.ArrayValue);

    foreach (var reference in ReferencesValueTest)
    {
      Assert.Contains(reference.Sys.Id, concrete.ReferenceIds);
    }
  }

  public static Dictionary<string, object?> WrapWithLocalisation(object? toWrap, string localisation = "en-US")
  => new()
  {
    [localisation] = toWrap
  };
}

public class JsonToDbMapperImplementation : JsonToDbMapper
{
  public JsonToDbMapperImplementation(Type entityType, ILogger logger, JsonSerializerOptions jsonSerialiserOptions) : base(entityType, logger, jsonSerialiserOptions)
  {
  }

  public override ContentComponentDbEntity MapEntity(CmsWebHookPayload payload)
  {
    var values = GetEntityValuesDictionary(payload);

    var asJson = JsonSerializer.Serialize(values, JsonOptions);
    var serialised = JsonSerializer.Deserialize<ContentComponentDbEntityImplementation>(asJson, JsonOptions) ?? throw new NullReferenceException("Null returned");

    return serialised;
  }
}

public class ContentComponentDbEntityImplementation : ContentComponentDbEntity
{
  public string StringValue { get; init; } = null!;
  public int NumberValue { get; init; }
  public string[] ArrayValue { get; init; } = null!;
  public string[] ReferenceIds { get; init; } = null!;
}