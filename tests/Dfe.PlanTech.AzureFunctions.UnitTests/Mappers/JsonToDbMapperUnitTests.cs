using System.Text.Json;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class JsonToDbMapperUnitTests : BaseMapperTests
{
    private const string StringValueTest = "string test";
    private readonly string[] ArrayValueTest = ["one", "two", "three"];
    private const int NumberValueTest = 10000;
    private const string EntityId = "Testing Id";

    private readonly CmsWebHookSystemDetailsInnerContainer[] ReferencesValueTest = [
          new CmsWebHookSystemDetailsInnerContainer()
          {
              Sys = new()
              {
                  Id = "First reference"
              }
          },
        new CmsWebHookSystemDetailsInnerContainer()
        {
            Sys = new()
            {
                Id = "Second reference"
            }
        }
        ];

    private readonly JsonToDbMapperImplementation _mapper;

    private readonly ILogger<JsonToDbMapper<ContentComponentImplementationDbEntity>> _logger = Substitute.For<ILogger<JsonToDbMapper<ContentComponentImplementationDbEntity>>>();

    public JsonToDbMapperUnitTests()
    {
        _mapper = new JsonToDbMapperImplementation(_logger, JsonOptions);
    }

    [Theory]
    [InlineData("ContentComponentImplementation", true)]
    [InlineData("Button", false)]
    [InlineData("ButtonWithEntryReference", false)]
    public void AcceptsContentType_Should_Return_Correct_Output(string typeName, bool expectedResult)
    {
        var acceptsContentType = _mapper.AcceptsContentType(typeName);

        Assert.Equal(expectedResult, acceptsContentType);
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
        CmsWebHookPayload payload = CreatePayload(fields, EntityId);

        var mapped = _mapper.ToEntity(payload);

        Assert.NotNull(mapped);

        var concrete = mapped;
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
}

public class JsonToDbMapperImplementation : JsonToDbMapper<ContentComponentImplementationDbEntity>
{
    public JsonToDbMapperImplementation(ILogger<JsonToDbMapper<ContentComponentImplementationDbEntity>> logger, JsonSerializerOptions jsonSerialiserOptions) : base(MapperHelpers.CreateMockEntityRetriever(), MapperHelpers.CreateMockEntityUpdater(), logger, jsonSerialiserOptions)
    {
    }

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        return values;
    }
}

public class ContentComponentImplementationDbEntity : ContentComponentDbEntity
{
    public string StringValue { get; init; } = null!;
    public int NumberValue { get; init; }
    public string[] ArrayValue { get; init; } = null!;
    public string[] ReferenceIds { get; init; } = null!;
}
