using System.Text.Json;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class JsonToEntityMappers
{
    private readonly JsonSerializerOptions _jsonSerialiserOptions;
    private readonly HashSet<JsonToDbMapper> _mappers;

    public JsonToEntityMappers(IEnumerable<JsonToDbMapper> mappers, JsonSerializerOptions jsonSerialiserOptions)
    {
        _jsonSerialiserOptions = jsonSerialiserOptions;
        _mappers = mappers.ToHashSet();
    }

    public ContentComponentDbEntity ToEntity(string requestBody)
    {
        var payload = SerialiseToPayload(requestBody);

        JsonToDbMapper mapper = GetMapperForPayload(payload);

        return mapper.MapEntity(payload);
    }

    private JsonToDbMapper GetMapperForPayload(CmsWebHookPayload payload)
    {
        var contentType = payload.Sys.ContentType.Sys.Id;

        return _mappers.FirstOrDefault(mapper => mapper.AcceptsContentType(contentType)) ?? throw new Exception($"Could not find mapper for {contentType}");
    }

    private CmsWebHookPayload SerialiseToPayload(string requestBody)
      => JsonSerializer.Deserialize<CmsWebHookPayload>(requestBody, _jsonSerialiserOptions) ?? throw new Exception($"Could not serialise body to {typeof(CmsWebHookPayload)}. Body was {requestBody}");
}