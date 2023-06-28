using Microsoft.Extensions.Logging;
using Moq;

namespace Dfe.PlanTech.Web.UnitTests;

public static class LoggerMock
{
    public static System.Linq.Expressions.Expression<Action<ILogger<T>>> LogMethod<T>()
                => logger => logger.Log(LogLevel.Warning,
                                        It.IsAny<EventId>(),
                                        It.IsAny<It.IsAnyType>(),
                                        It.IsAny<Exception>(),
                                        It.IsAny<Func<It.IsAnyType, Exception?, string>>());
}