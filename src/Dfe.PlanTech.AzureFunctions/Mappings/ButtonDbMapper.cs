namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class ButtonDbMapper : JsonToDbMapper<ButtonDbEntity>
{
    public ButtonDbMapper(ILogger<ButtonDbMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : base(logger, jsonSerialiserOptions)
    {
    }

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        return values;
    }
}