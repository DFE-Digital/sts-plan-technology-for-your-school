using System.Text.Json;
using Dfe.PlanTech.Infrastructure.ServiceBus.Interfaces;
using Dfe.PlanTech.Infrastructure.ServiceBus.Queueing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.ServiceBus.Commands;

/// <summary>
/// Processes the HTTP request/body from the Contentful webhook, and writes it to an Azure Service Bus queue using the <see cref="IQueueWriter"/>
/// </summary>
/// <param name="queueWriter"></param>
/// <param name="logger"></param>
public class WriteCmsWebhookToQueueCommand(
    ILoggerFactory loggerFactory,
    IQueueWriter queueWriter
) : IWriteCmsWebhookToQueueCommand
{
    public const string ContentfulTopicHeaderKey = "X-Contentful-Topic";

    private readonly ILogger<WriteCmsWebhookToQueueCommand> _logger = loggerFactory.CreateLogger<WriteCmsWebhookToQueueCommand>();
    private readonly IQueueWriter _queueWriter = queueWriter ?? throw new ArgumentNullException(nameof(queueWriter));

    public async Task<QueueWriteResult> WriteMessageToQueue(JsonDocument json, HttpRequest request)
    {
        try
        {
            _logger.LogTrace("Received CMS webhook payload");

            var cmsEvent = GetCmsEvent(request);
            if (cmsEvent == null)
            {
                var errorMessage = $"Couldn't find header {ContentfulTopicHeaderKey}";
                _logger.LogError("Couldn't find header {ContentfulTopicHeaderKey}", ContentfulTopicHeaderKey);
                return new QueueWriteResult(errorMessage);
            }

            _logger.LogTrace("CMS Event is {CmsEvent}", cmsEvent);

            var body = JsonSerializer.Serialize(json);

            _logger.LogTrace("Message body is {Body}", body);
            return await _queueWriter.WriteMessage(body, cmsEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save message to queue");
            return new QueueWriteResult(ex.Message);
        }
    }

    private static string? GetCmsEvent(HttpRequest request) =>
        !request.Headers.TryGetValue(ContentfulTopicHeaderKey,
            out var value)
            ? null
            : value.FirstOrDefault();
}
