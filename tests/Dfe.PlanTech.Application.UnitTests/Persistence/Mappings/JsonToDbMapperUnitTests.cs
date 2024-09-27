using System.Text.Json;
using System.Text.Json.Nodes;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Persistence.Mappings;

public class JsonToDbMapperUnitTests : BaseMapperTests
{

    private const string StringValueTest = "string test";
    private readonly string[] ArrayValueTest = ["one", "two", "three"];
    private const int NumberValueTest = 10000;
    private const string EntityId = "Testing Id";

    private readonly CmsWebHookSystemDetailsInnerContainer[] ReferencesValueTest =
    [
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

    private readonly BaseJsonToDbMapperImplementation _mapper;

    private readonly ILogger<BaseJsonToDbMapper<ContentComponentImplementationDbEntity>> _logger =
        Substitute.For<ILogger<BaseJsonToDbMapper<ContentComponentImplementationDbEntity>>>();

    public JsonToDbMapperUnitTests()
    {
        _mapper = new BaseJsonToDbMapperImplementation(EntityUpdater, _logger, JsonOptions, DatabaseHelper);
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

    [Fact]
    public void GetValuesFromFields_ShouldLogErrorAndReturnNull_WhenFieldValueIsNull()
    {
        var field = new KeyValuePair<string, JsonNode>("testField", null!);

        var result = _mapper.TestGetValuesFromFields(field).ToList();

        Assert.Single(result);
        Assert.False(result[0].HasValue);

        var logMessages = _logger.GetMatchingReceivedMessages($"No value for {field.Key}", LogLevel.Error);
        Assert.Single(logMessages);
    }

    [Fact]
    public void GetValuesFromFields_Should_NotReturn_Anything()
    {
        var field = new KeyValuePair<string, JsonNode>("testField", CreateJsonNode()); // Replace with a valid JsonNode

        var result = _mapper.TestGetValuesFromFields(field).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void GetValuesFromFields_Should_LogError_When_MoreThanOneChild()
    {
        var json = """{ "en-gb": {"child1": "value"}, "en-otherlanguage": 5}""";
        var field = new KeyValuePair<string, JsonNode>("testField", CreateJsonNode(json)); // Replace with a valid JsonNode

        var result = _mapper.TestGetValuesFromFields(field).ToList();

        Assert.Single(result);

        var first = result[0];

        Assert.False(first.HasValue);
        var receivedLoggerMessages = _logger.GetMatchingReceivedMessages("Expected only one language - received 2", LogLevel.Error).ToArray();

        Assert.Single(receivedLoggerMessages);
    }

    [Fact]
    public void GetValueAsObject_Handles_NonObjects()
    {
        var arg = CreateJsonNodeFromArray();

        var result = _mapper.InvokeNonPublicMethod<BaseJsonToDbMapper>("GetValueAsObject", new[] { arg });

        Assert.Null(result);

        var loggedMessages = _logger.ReceivedLogMessages().ToArray();

        Assert.Single(loggedMessages);
        Assert.Equal(LogLevel.Error, loggedMessages[0].LogLevel);
        Assert.Contains("Error when serialising field", loggedMessages[0].Message);
    }

    [Fact]
    public void GetValuesFromFields_Handles_NullChildren()
    {
        var arg = CreateJsonNodeFromArray();

        var result = ((IEnumerable<KeyValuePair<string, object?>?>)_mapper.InvokeNonPublicMethod<BaseJsonToDbMapper>("GetValuesFromFields", new[] { arg })).ToArray();

        Assert.Single(result);
        Assert.True(result[0].HasValue);
        Assert.Null(result[0]!.Value.Value);

        var loggedMessages = _logger.ReceivedLogMessages().ToArray();

        Assert.Single(loggedMessages);
        Assert.Equal(LogLevel.Error, loggedMessages[0].LogLevel);
        Assert.Contains("Error when serialising field", loggedMessages[0].Message);
    }

    private static object CreateJsonNodeFromArray()
    {
        var jsonArray = "[\"Key\", \"Value\"]";
        JsonNode? jsonNode = JsonNode.Parse(jsonArray);
        Assert.NotNull(jsonNode);

        return new KeyValuePair<string, JsonNode>("Testing key", jsonNode!);
    }

    private static JsonNode CreateJsonNode(string json = "{}")
    {
        var jsonObj = JsonSerializer.Deserialize<JsonNode>(json);
        return jsonObj!;
    }
}

public class BaseJsonToDbMapperImplementation : BaseJsonToDbMapper<ContentComponentImplementationDbEntity>
{
    public BaseJsonToDbMapperImplementation(EntityUpdater entityUpdater,
                                        ILogger<BaseJsonToDbMapper<ContentComponentImplementationDbEntity>> logger,
                                        JsonSerializerOptions jsonSerialiserOptions,
                                        IDatabaseHelper<ICmsDbContext> databaseHelper) : base(entityUpdater, logger, jsonSerialiserOptions, databaseHelper)
    {
    }

    protected override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        return values;
    }

    public IEnumerable<KeyValuePair<string, object?>?> TestGetValuesFromFields(KeyValuePair<string, JsonNode> field)
    {
        return GetValuesFromFields(field);
    }
}

public class ContentComponentImplementationDbEntity : ContentComponentDbEntity
{
    public string StringValue { get; init; } = null!;
    public int NumberValue { get; init; }
    public string[] ArrayValue { get; init; } = null!;
    public string[] ReferenceIds { get; init; } = null!;
}
