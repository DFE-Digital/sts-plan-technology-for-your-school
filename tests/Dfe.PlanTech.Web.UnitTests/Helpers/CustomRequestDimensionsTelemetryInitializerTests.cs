using Dfe.PlanTech.Web.Helpers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;

public class CustomRequestDimensionsTelemetryInitializerTests
{
    [Fact]
    public void Initialize_Sets_RequestId_From_HttpContext_TraceIdentifier()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        ctx.TraceIdentifier = "trace-123";
        var accessor = new HttpContextAccessor { HttpContext = ctx };
        var sut = new CustomRequestDimensionsTelemetryInitializer(accessor);

        var telemetry = new RequestTelemetry();

        // Act
        sut.Initialize(telemetry);

        // Assert
        Assert.True(telemetry.Properties.TryGetValue("RequestId", out var value));
        Assert.Equal("trace-123", value);
    }

    [Fact]
    public void Initialize_Overwrites_Existing_RequestId()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        ctx.TraceIdentifier = "new-trace";
        var accessor = new HttpContextAccessor { HttpContext = ctx };
        var sut = new CustomRequestDimensionsTelemetryInitializer(accessor);

        var telemetry = new RequestTelemetry();
        telemetry.Properties["RequestId"] = "old-trace";

        // Act
        sut.Initialize(telemetry);

        // Assert
        Assert.Equal("new-trace", telemetry.Properties["RequestId"]);
    }

    private sealed class DummyTelemetry : ITelemetry
    {
        public DateTimeOffset Timestamp { get; set; }
        public TelemetryContext Context { get; } = new();
        public string Sequence { get; set; } = "";
        public IExtension Extension { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ITelemetry DeepClone()
        {
            throw new NotImplementedException();
        }

        public void Sanitize() { }

        public void SerializeData(ISerializationWriter serializationWriter)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public void Initialize_Ignores_NonRequestTelemetry()
    {
        // Arrange
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var sut = new CustomRequestDimensionsTelemetryInitializer(accessor);
        ITelemetry telemetry = new DummyTelemetry();

        // Act (no throw)
        sut.Initialize(telemetry);

        // Assert: nothing to assert; just verifying it’s a no-op for non-request telemetry
    }
}
