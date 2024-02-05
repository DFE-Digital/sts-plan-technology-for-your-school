namespace Dfe.PlanTech.AzureFunctions.UnitTests;

public class TitleMapperUnitTests : BaseMapperTests
{
    private const string TitleText = "Title text";
    private const string TitleId = "Title Id";

    private readonly TitleMapper _mapper;
    private readonly ILogger<TitleMapper> _logger;

    public TitleMapperUnitTests()
    {
        _logger = Substitute.For<ILogger<TitleMapper>>();
        _mapper = new TitleMapper(_logger, JsonOptions);
    }

    [Fact]
    public void Mapper_Should_Map_NavigationLink()
    {
        var fields = new Dictionary<string, object?>()
        {
            ["text"] = WrapWithLocalisation(TitleText),
        };

        var payload = CreatePayload(fields, TitleId);

        var mapped = _mapper.MapEntity(payload);

        Assert.NotNull(mapped);

        var concrete = mapped as TitleDbEntity;
        Assert.NotNull(concrete);

        Assert.Equal(TitleId, concrete.Id);
        Assert.Equal(TitleText, concrete.Text);
    }
}