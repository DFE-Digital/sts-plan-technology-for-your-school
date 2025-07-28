using System.Text.Json;
using Dfe.PlanTech.Core.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Infrastructure.ServiceBus.Results;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Commands;

public class CmsWebHookMessageProcessor(
    ILoggerFactory loggerFactory,
    ICmsCache cache,
    JsonSerializerOptions jsonSerialiserOptions
)
{
    private readonly ILogger<CmsWebHookMessageProcessor> _logger = loggerFactory.CreateLogger<CmsWebHookMessageProcessor>();
    private readonly ICmsCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly JsonSerializerOptions _jsonSerialiserOptions = jsonSerialiserOptions ?? throw new ArgumentNullException(nameof(jsonSerialiserOptions));

    public async Task<ServiceBusResult> ProcessMessage(string subject, string body, string id, CancellationToken cancellationToken)
    {
        try
        {
            var payload = MapMessageToPayload(body);

            await _cache.InvalidateCacheAsync(payload.Sys.Id, payload.ContentType);
            return new ServiceBusSuccessResult();
        }
        catch (Exception ex) when (ex is JsonException)
        {
            _logger.LogError(ex, "Error processing message ID {Message}", id);
            return new ServiceBusErrorResult(ex.Message, ex.StackTrace, false);
        }
        catch (Exception ex)
        {
            return new ServiceBusErrorResult(ex.Message, ex.StackTrace, true);
        }
    }

    /// <summary>
    /// Maps the incoming message to a CmsWebHookPayload
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    private CmsWebHookPayload MapMessageToPayload(string body)
    {
        _logger.LogInformation("Processing mesasge:\n{MessageBody}", body);

        return JsonSerializer.Deserialize<CmsWebHookPayload>(body, _jsonSerialiserOptions)
            ?? throw new InvalidOperationException($"Could not serialise body to {typeof(CmsWebHookPayload)}. Body was {body}");
    }
}
