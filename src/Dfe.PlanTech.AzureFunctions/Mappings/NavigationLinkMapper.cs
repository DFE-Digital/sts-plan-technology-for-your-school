namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class NavigationLinkMapper : JsonToDbMapper<NavigationLinkDbEntity>
{
    public NavigationLinkMapper(ILogger<NavigationLinkMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : base(logger, jsonSerialiserOptions)
    {
    }

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        return values;
    }
}