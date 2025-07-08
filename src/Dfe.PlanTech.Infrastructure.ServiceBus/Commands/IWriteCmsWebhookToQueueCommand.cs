using System.Text.Json;
using Dfe.PlanTech.Infrastructure.ServiceBus.Queueing;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Infrastructure.ServiceBus.Commands;

/// <summary>
/// Processes webhook payloads from Contentful and saves them to the DB
/// </summary>
public interface IWriteCmsWebhookToQueueCommand
{
    /// <summary>
    /// Writes CMS webhook payload to queue
    /// </summary>
    /// <param name="json"></param>
    /// <param name="request"></param>
    /// <returns>Error message if any</returns>
    public Task<QueueWriteResult> WriteMessageToQueue(JsonDocument json, HttpRequest request);
}
