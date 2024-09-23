namespace Dfe.PlanTech.Domain.ServiceBus.Models;

public class MessageRetryHandlingOptions
{
    public int MaxMessageDeliveryAttempts { get; set; } = 4;
    public int MessageDeliveryDelayInSeconds { get; set; } = 10;
}
