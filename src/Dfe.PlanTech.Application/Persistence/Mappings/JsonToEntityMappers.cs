using System.Text.Json;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class JsonToEntityMappers(IEnumerable<JsonToDbMapper> mappers, JsonSerializerOptions jsonSerialiserOptions)
{
    private readonly JsonSerializerOptions _jsonSerialiserOptions = jsonSerialiserOptions;
    private readonly HashSet<JsonToDbMapper> _mappers = mappers.ToHashSet();

    public Task<MappedEntity> ToEntity(string requestBody, CmsEvent cmsEvent, CancellationToken cancellationToken)
    {
        var payload = SerialiseToPayload(requestBody);

        var mapper = GetMapperForPayload(payload);

        return mapper.MapEntity(payload, cmsEvent, cancellationToken);
    }

    private JsonToDbMapper GetMapperForPayload(CmsWebHookPayload payload)
        => _mappers.FirstOrDefault(mapper => mapper.AcceptsContentType(payload.ContentType)) ?? throw new KeyNotFoundException($"Could not find mapper for {payload.ContentType}");

    private CmsWebHookPayload SerialiseToPayload(string requestBody)
        => JsonSerializer.Deserialize<CmsWebHookPayload>(requestBody, _jsonSerialiserOptions) ?? throw new InvalidOperationException($"Could not serialise body to {typeof(CmsWebHookPayload)}. Body was {requestBody}");
}
