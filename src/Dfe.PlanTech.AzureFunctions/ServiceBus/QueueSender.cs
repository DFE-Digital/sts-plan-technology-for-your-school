using System.Text.Json;
using Azure.Identity;
using Azure.Messaging.ServiceBus;

namespace Dfe.PlanTech.AzureFunctions.ServiceBus;

public class QueueSender : QueueUser, IAsyncDisposable
{
  private readonly ServiceBusSender _sender;

  public QueueSender(ServiceBusClient client, string queueName) : base(client)
  {
    _sender = client.CreateSender(queueName);
  }

  public async Task SendMessage<T>(T message)
  {
    var json = JsonSerializer.Serialize(message);
    var serviceBusMessage = new ServiceBusMessage(json);

    await _sender.SendMessageAsync(serviceBusMessage);
  }
}