using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Domain.Content.Models.Buttons;

namespace Dfe.PlanTech.Application.UnitTests.Persistence.Mappings;

public class ButtonMapperTests : BaseMapperTests<ButtonDbEntity, ButtonMapper>
{
    private const string ButtonId = "ButtonId Id";

    private readonly ButtonMapper _mapper;

    public ButtonMapperTests()
    {
        _mapper = new ButtonMapper(EntityUpdater, Logger, JsonOptions, DatabaseHelper);
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
