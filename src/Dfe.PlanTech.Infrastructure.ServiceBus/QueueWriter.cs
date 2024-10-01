using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Application.Queues.Interfaces;
using Dfe.PlanTech.Domain.Queues.Models;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.ServiceBus;

/// <summary>
/// Implementation of <see cref="IQueueWriter"/> for the Azure Service Bus
/// </summary>
/// <param name="serviceBusSenderFactory"></param>
/// <param name="logger"></param>
public class QueueWriter(IAzureClientFactory<ServiceBusSender> serviceBusSenderFactory, ILogger<QueueWriter> logger) : IQueueWriter
{
    private readonly ServiceBusSender _serviceBusSender = serviceBusSenderFactory.CreateClient("contentfulsender");

    public async Task<QueueWriteResult> WriteMessage(string body, string subject)
    {
        try
        {
            var serviceBusMessage = new ServiceBusMessage(body) { Subject = subject };
            await _serviceBusSender.SendMessageAsync(serviceBusMessage);
            return new QueueWriteResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending service bus message. Body: \"{Body}\". Subject: \"{Subject}\".", body, subject);
            return new QueueWriteResult(ex.Message);
        }
    }
}
