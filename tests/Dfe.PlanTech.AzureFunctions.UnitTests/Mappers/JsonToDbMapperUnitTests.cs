namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class JsonToDbMapperUnitTests : BaseMapperTests
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

    private readonly Type _type = typeof(ButtonWithLinkDbEntity);

    private readonly ILogger _logger = Substitute.For<ILogger>();

    public JsonToDbMapperUnitTests()
    {
        _mapper = new JsonToDbMapperImplementation(_type, _logger, JsonOptions);
    }

    [Theory]
    [InlineData("ButtonWithLink", true)]
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
