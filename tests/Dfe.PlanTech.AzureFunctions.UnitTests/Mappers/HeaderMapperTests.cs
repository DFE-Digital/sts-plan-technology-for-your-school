namespace Dfe.PlanTech.AzureFunctions.UnitTests;

public class HeaderMapperTests : BaseMapperTests
{
    private const string HeaderText = "Header text goes here";
    private const HeaderSize HeaderSizeValue = HeaderSize.Medium;
    private const HeaderTag HeaderTagValue = HeaderTag.H2;
    private const string HeaderId = "Header Id";

    private readonly HeaderMapper _mapper;
    private readonly ILogger<JsonToDbMapper<HeaderDbEntity>> _logger;
    public HeaderMapperTests()
    {
        _logger = Substitute.For<ILogger<JsonToDbMapper<HeaderDbEntity>>>();
        _mapper = new HeaderMapper(_logger, JsonOptions);
    }

    [Fact]
    public void Mapper_Should_Map_Relationship()
    {
        var fields = new Dictionary<string, object?>()
        {
            ["text"] = WrapWithLocalisation(HeaderText),
            ["size"] = WrapWithLocalisation(HeaderSizeValue),
            ["tag"] = WrapWithLocalisation(HeaderTagValue),
        };

        var payload = CreatePayload(fields, HeaderId);

        var mapped = _mapper.MapEntity(payload);

        Assert.NotNull(mapped);

        var concrete = mapped as HeaderDbEntity;
        Assert.NotNull(concrete);

        Assert.Equal(HeaderId, concrete.Id);
        Assert.Equal(HeaderText, concrete.Text);
        Assert.Equal(HeaderSizeValue, concrete.Size);
        Assert.Equal(HeaderTagValue, concrete.Tag);
    }
}