using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Application.Queues.Interfaces;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.ServiceBus;

public class QueueWriter(IAzureClientFactory<ServiceBusSender> serviceBusSenderFactory, ILogger<QueueWriter> logger) : IQueueWriter
{
    private readonly ServiceBusSender _serviceBusSender = serviceBusSenderFactory.CreateClient("contentfulsender");

    public async Task WriteMessage(string body, string subject)
    {
        try
        {
            var serviceBusMessage = new ServiceBusMessage(body) { Subject = subject };
            await _serviceBusSender.SendMessageAsync(serviceBusMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending service bus message. Body: \"{Body}\". Subject: \"{Subject}\".", body, subject);
            throw;
        }
    }
}
