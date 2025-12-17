using Dfe.PlanTech.Infrastructure.ServiceBus.Interfaces;

namespace Dfe.PlanTech.Infrastructure.ServiceBus.Results;

public class ServiceBusErrorResult : IServiceBusResult
{
    public string Reason { get; set; }
    public string? Description { get; set; }
    public bool IsRetryable { get; set; }

    public ServiceBusErrorResult(string reason, string? description, bool isRetryable)
    {
        Reason = reason;
        Description = description;
        IsRetryable = isRetryable;
    }
}
