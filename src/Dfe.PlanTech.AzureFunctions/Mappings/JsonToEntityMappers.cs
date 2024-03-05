using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using System.Text.Json;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class JsonToEntityMappers
{
    private readonly JsonSerializerOptions _jsonSerialiserOptions;
    private readonly HashSet<JsonToDbMapper> _mappers;
    private JsonToDbMapper? _mapper;

    public JsonToEntityMappers(IEnumerable<JsonToDbMapper> mappers, JsonSerializerOptions jsonSerialiserOptions)
    {
        _jsonSerialiserOptions = jsonSerialiserOptions;
        _mappers = mappers.ToHashSet();
    }

    public Task<MappedEntity> ToEntity(string requestBody, CmsEvent cmsEvent, CancellationToken cancellationToken)
    {
        var payload = SerialiseToPayload(requestBody);

        JsonToDbMapper mapper = GetMapperForPayload(payload);

        return mapper.MapEntity(payload, cmsEvent, cancellationToken);
    }

    private JsonToDbMapper GetMapperForPayload(CmsWebHookPayload payload)
    {
        var contentType = payload.Sys.ContentType.Sys.Id;

        return _mappers.FirstOrDefault(mapper => mapper.AcceptsContentType(contentType)) ?? throw new KeyNotFoundException($"Could not find mapper for {contentType}");
    }

    private CmsWebHookPayload SerialiseToPayload(string requestBody)
      => JsonSerializer.Deserialize<CmsWebHookPayload>(requestBody, _jsonSerialiserOptions) ?? throw new InvalidOperationException($"Could not serialise body to {typeof(CmsWebHookPayload)}. Body was {requestBody}");
}