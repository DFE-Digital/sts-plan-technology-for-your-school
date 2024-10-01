using Dfe.PlanTech.Domain.ServiceBus.Models;

namespace Dfe.PlanTech.Domain.Persistence.Interfaces;

/// <summary>
/// Processes Contentful webhook payloads, and saves them to the DB
/// </summary>
public interface IWebhookToDbCommand
{
    /// <summary>
    /// Validates, maps, and handles CRUD operations of a Contentful webhook payload to the DB
    /// </summary>
    /// <param name="subject">The CMS event <see cref="Caching.Enums.CmsEvent"/></param>
    /// <param name="body">The CMS webhook payload (JSON string)</param>
    /// <param name="id">The queue message ID</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IServiceBusResult> ProcessMessage(string subject, string body, string id, CancellationToken cancellationToken);
}
