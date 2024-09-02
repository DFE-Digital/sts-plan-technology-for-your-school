using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech;

public static class LoggingHelpers
{
    public static IEnumerable<ReceivedLoggerCall> GetMatchingReceivedMessages<T>(this ILogger<T> logger, string message,
        LogLevel logLevel)
        => logger.ReceivedCalls().Select(call =>
        {
            var args = call.GetArguments();

            var msg = args[2]?.ToString() ?? "";

            var level = Enum.Parse<LogLevel>(args[0]?.ToString() ?? "");

            return new ReceivedLoggerCall(level, msg);
        }).Where(LogMessageMatches(message, logLevel));

    private static Func<ReceivedLoggerCall, bool> LogMessageMatches(string message, LogLevel logLevel)
    => args => args.Message.Equals(message) && args.LogLevel == logLevel;
}

public record ReceivedLoggerCall(LogLevel LogLevel, string Message)
{

}
