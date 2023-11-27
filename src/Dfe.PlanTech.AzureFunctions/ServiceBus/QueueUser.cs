using Azure.Messaging.ServiceBus;

namespace Dfe.PlanTech.AzureFunctions.ServiceBus;

public class QueueUser : IAsyncDisposable
{
  protected readonly ServiceBusClient client;

  public QueueUser(ServiceBusClient client)
  {
    this.client = client;
  }

  public async ValueTask DisposeAsync()
  {
    await client.DisposeAsync();
  }
}