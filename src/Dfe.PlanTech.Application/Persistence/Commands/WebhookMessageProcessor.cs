using System.Text.Json;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.ServiceBus.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Commands;

/// <inheritdoc cref="IWebhookToDbCommand" />
public class WebhookMessageProcessor(ICmsCache cache,
                                    JsonSerializerOptions jsonSerialiserOptions,
                                    ILogger<WebhookMessageProcessor> logger) : IWebhookToDbCommand
{
    public async Task<IServiceBusResult> ProcessMessage(string subject, string body, string id, CancellationToken cancellationToken)
    {
        try
        {
            var payload = MapMessageToPayload(body);

            await cache.InvalidateCacheAsync(payload.Sys.Id);
            return new ServiceBusSuccessResult();
        }
        catch (Exception ex) when (ex is JsonException)
        {
            logger.LogError(ex, "Error processing message ID {Message}", id);
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
        logger.LogInformation("Processing mesasge:\n{MessageBody}", body);

        return JsonSerializer.Deserialize<CmsWebHookPayload>(body, jsonSerialiserOptions) ?? throw new InvalidOperationException($"Could not serialise body to {typeof(CmsWebHookPayload)}. Body was {body}");
    }
}
