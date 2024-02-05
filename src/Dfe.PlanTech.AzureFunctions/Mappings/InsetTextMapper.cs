namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class InsetTextMapper : JsonToDbMapper<InsetTextDbEntity>
{
    public InsetTextMapper(ILogger<InsetTextMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : base(logger, jsonSerialiserOptions)
    {
    }

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        return values;
    }
}