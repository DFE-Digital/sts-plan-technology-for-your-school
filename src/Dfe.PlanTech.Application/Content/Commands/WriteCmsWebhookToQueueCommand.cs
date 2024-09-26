using System.Text.Json;
using Dfe.PlanTech.Application.Queues.Interfaces;
using Dfe.PlanTech.Domain.Queues.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Commands;

/// <summary>
/// Processes the HTTP request/body from the Contentful webhook, and writes it to an Azure Service Bus queue using the <see cref="IQueueWriter"/>
/// </summary>
/// <param name="queueWriter"></param>
/// <param name="logger"></param>
public class WriteCmsWebhookToQueueCommand(IQueueWriter queueWriter, ILogger<WriteCmsWebhookToQueueCommand> logger) : IWriteCmsWebhookToQueueCommand
{
    public const string ContentfulTopicHeaderKey = "X-Contentful-Topic";

    public async Task<QueueWriteResult> WriteMessageToQueue(JsonDocument json, HttpRequest request)
    {
        try
        {
            logger.LogTrace("Received CMS webhook payload");

            var cmsEvent = GetCmsEvent(request);
            if (cmsEvent == null)
            {
                var errorMessage = $"Couldn't find header {ContentfulTopicHeaderKey}";
                logger.LogError("Couldn't find header {ContentfulTopicHeaderKey}", ContentfulTopicHeaderKey);
                return new QueueWriteResult(errorMessage);
            }

            logger.LogTrace("CMS Event is {CmsEvent}", cmsEvent);

            var body = JsonSerializer.Serialize(json);

            logger.LogTrace("Message body is {Body}", body);
           return await queueWriter.WriteMessage(body, cmsEvent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save message to queue");
            return new QueueWriteResult(ex.Message);
        }
    }

    private static string? GetCmsEvent(HttpRequest request) =>
        !request.Headers.TryGetValue(ContentfulTopicHeaderKey,
            out var value)
            ? null
            : value.FirstOrDefault();
}
