using System.Text.Json;
using Dfe.PlanTech.Application.Queues.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Commands;

public class WriteCmsWebhookToQueueCommand(ILogger<WriteCmsWebhookToQueueCommand> logger, IQueueWriter queueWriter) : IWriteCmsWebhookToQueueCommand
{
    private const string ContentfulTopicHeaderKey = "X-Contentful-Topic";

    public async Task<string?> WriteMessageToQueue(JsonDocument json, HttpRequest request)
    {
        try
        {
            logger.LogTrace("Received CMS webhook payload");

            var cmsEvent = GetCmsEvent(request);
            if (cmsEvent == null)
            {
                var errorMessage = $"Couldn't find header {ContentfulTopicHeaderKey}";
                logger.LogError(errorMessage);
                return errorMessage;
            }

            logger.LogTrace("CMS Event is {CmsEvent}", cmsEvent);

            var body = JsonSerializer.Serialize(json);

            logger.LogTrace("Message body is {Body}", body);
            await queueWriter.WriteMessage(body, cmsEvent);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save message to queue");
            throw;
        }

    }

    private static string? GetCmsEvent(HttpRequest request)
    {
        return !request.Headers.TryGetValue(ContentfulTopicHeaderKey, out var value) ? null : value.FirstOrDefault();
    }
}
