namespace Dfe.PlanTech.AzureFunctions.Config;

public class MessageRetryHandlingOptions
{
    public int MaxMessageDeliveryAttempts { get; set; } = 4;
    public int MessageDeliveryDelayInSeconds { get; set; } = 10;
}
