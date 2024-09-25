using Dfe.PlanTech.Domain.ServiceBus.Models;

namespace Dfe.PlanTech.Domain.Persistence.Interfaces
{
    /// <summary>
    /// Processes Contentful webhook payloads, and saves them to the DB
    /// </summary>
    public interface IWebhookToDbCommand
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<IServiceBusResult> ProcessMessage(string subject, string body, string id, CancellationToken cancellationToken);
    }
}
