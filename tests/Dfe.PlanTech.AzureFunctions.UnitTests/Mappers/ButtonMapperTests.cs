using Dfe.PlanTech.AzureFunctions.Mappings;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class ButtonMapperTests : BaseMapperTests
{
    private const string ButtonId = "ButtonId Id";

    private readonly ButtonDbMapper _mapper;
    private readonly ILogger<ButtonDbMapper> _logger;

    public ButtonMapperTests()
    {
        _logger = Substitute.For<ILogger<ButtonDbMapper>>();
        _mapper = new ButtonDbMapper(MapperHelpers.CreateMockEntityRetriever(), MapperHelpers.CreateMockEntityUpdater(), _logger, JsonOptions);
    }

    [Theory]
    [InlineData("Button value", true)]
    [InlineData("Button value", false)]
    [InlineData("", true)]
    [InlineData("", false)]
    public void Mapper_Should_Map_Button(string buttonValue, bool isStartButton)
    {
        var fields = new Dictionary<string, object?>()
        {
            ["value"] = WrapWithLocalisation(buttonValue),
            ["isStartButton"] = WrapWithLocalisation(isStartButton),
        };

        var payload = CreatePayload(fields, ButtonId);

        var mapped = _mapper.ToEntity(payload);

        Assert.NotNull(mapped);

        var concrete = mapped;
        Assert.NotNull(concrete);

        Assert.Equal(ButtonId, concrete.Id);
        Assert.True(string.Equals(buttonValue.ToString(), concrete.Value, StringComparison.InvariantCultureIgnoreCase));
        Assert.Equal(isStartButton, concrete.IsStartButton);
    }
}