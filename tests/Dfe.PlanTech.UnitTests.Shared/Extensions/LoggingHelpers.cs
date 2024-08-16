using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech;

public static class LoggingHelpers
{
    public static void ReceivedMessages<T>(this ILogger<T> logger, string message, LogLevel logLevel, int receivedCount)
    {
        var receivedCalls = logger.ReceivedCalls().Select(call =>
        {
            var args = call.GetArguments();

            var message = args[2]?.ToString();

            var logLevel = Enum.Parse<LogLevel>(args[0]?.ToString() ?? "");

            return new
            {
                logLevel,
                message
            };
        }).ToArray();

        var matchingCallCount = receivedCalls.Where(args => args.message == message && args.logLevel == logLevel).Count();
        if (receivedCalls.Length != receivedCount)
        {
            var actualReceivedCalls = string.Join("\n", receivedCalls.Select(call => $"[{call.logLevel}]Message: {call.message}"));
            Assert.Fail($"Expected {receivedCount} logging messages to match but found {receivedCalls.Length} matching calls. Received calls: \n" + actualReceivedCalls);
        }
    }
}
