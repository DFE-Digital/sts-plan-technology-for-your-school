using System.Diagnostics;
using Dfe.PlanTech.Web.Helpers;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;


public class CustomTelemetryInitializerTests
{
    private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    
    [Fact]
    public void CustomInitializerAddsRequestIdToRequestTelemetry()
    {
        var telemetry = new RequestTelemetry();
        var customInitializer = new CustomTelemetryInitializer(_httpContextAccessor);

        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "TestTraceId";
        _httpContextAccessor.HttpContext.Returns(httpContext);

        customInitializer.Initialize(telemetry);

        Assert.True(telemetry.Properties.TryGetValue("RequestId", out var requestId));
        Assert.Equal("TestTraceId", requestId);
    }

    [Fact]
    public void CustomInitializerDoesNotAddRequestIdToNonRequestTelemetry()
    {
        var telemetry = new DependencyTelemetry();
        var customInitializer = new CustomTelemetryInitializer(_httpContextAccessor);

        customInitializer.Initialize(telemetry);

        Assert.False(telemetry.Properties.ContainsKey("RequestId"));
    }

    [Fact]
    public void CustomInitializerAddsRequestIdToRequestTelemetryWhereNoHttpContext()
    {
        var telemetry = new RequestTelemetry();
        var customInitializer = new CustomTelemetryInitializer(_httpContextAccessor);

        _httpContextAccessor.HttpContext.Returns((HttpContext)null);

        customInitializer.Initialize(telemetry);

        Assert.True(telemetry.Properties.TryGetValue("RequestId", out var requestId));
        Assert.Equal(Activity.Current?.Id, requestId);
    }
}
