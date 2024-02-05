namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class HeaderMapper : JsonToDbMapper<HeaderDbEntity>
{
    public HeaderMapper(ILogger<JsonToDbMapper<HeaderDbEntity>> logger, JsonSerializerOptions jsonSerialiserOptions) : base(logger, jsonSerialiserOptions)
    {
    }

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        return values;
    }
}