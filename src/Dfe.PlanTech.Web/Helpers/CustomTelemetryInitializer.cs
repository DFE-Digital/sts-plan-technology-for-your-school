using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Dfe.PlanTech.Web.Helpers;

public class CustomTelemetryInitializer : ITelemetryInitializer
{
    
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CustomTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is not RequestTelemetry requestTelemetry) return;
        var requestId = GetRequestId();
        requestTelemetry.Context.Properties["RequestId"] = requestId;
    }
    
    private string? GetRequestId()
    {
        return _httpContextAccessor.HttpContext?.TraceIdentifier;
    }
}