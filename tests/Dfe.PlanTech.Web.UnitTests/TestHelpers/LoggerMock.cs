using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests;

public static class LoggerMock
{
    public static System.Linq.Expressions.Expression<Action<ILogger<T>>> LogMethod<T>()
                => logger => logger.Log(LogLevel.Warning,
                                        Arg.Any<EventId>(),
                                        Arg.Any<Arg.AnyType>(),
                                        Arg.Any<Exception>(),
                                        Arg.Any<Func<Arg.AnyType, Exception?, string>>());
}