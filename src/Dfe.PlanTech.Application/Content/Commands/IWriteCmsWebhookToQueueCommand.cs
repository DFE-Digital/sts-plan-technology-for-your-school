using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Application.Content.Commands;

public interface IWriteCmsWebhookToQueueCommand
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="json"></param>
    /// <param name="request"></param>
    /// <returns>Error message if any</returns>
    public Task<string?> WriteMessageToQueue(JsonDocument json, HttpRequest request);
}
