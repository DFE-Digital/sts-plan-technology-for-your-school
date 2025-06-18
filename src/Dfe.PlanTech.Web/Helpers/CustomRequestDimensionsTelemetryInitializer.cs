using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Dfe.PlanTech.Web.Helpers;

public class CustomRequestDimensionsTelemetryInitializer : ITelemetryInitializer
{

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CustomRequestDimensionsTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is not RequestTelemetry requestTelemetry)
            return;
        var requestId = GetRequestId();
        requestTelemetry.Properties["RequestId"] = requestId;
    }

    private string? GetRequestId()
    {
        return _httpContextAccessor.HttpContext?.TraceIdentifier;
    }
}
