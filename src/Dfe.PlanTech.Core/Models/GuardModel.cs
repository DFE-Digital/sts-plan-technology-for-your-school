using Dfe.PlanTech.Core.Exceptions;

namespace Dfe.PlanTech.Core.Models;

public static class GuardModel
{
    public static void IsNotNull(object? obj, string name) =>
        IsTrue(obj != null, name + " cannot be null");

    public static void IsNotNull(object? obj, Func<Exception> exceptionFactory) =>
        IsTrue(obj != null, exceptionFactory);

    public static void IsTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new GuardException(message);
        }
    }

    public static void IsTrue<T>(T value, Func<T, bool> constraint, string message)
    {
        if (!constraint(value))
        {
            throw new GuardException(string.Format(message, value));
        }
    }

    public static void IsTrue<T>(bool condition)
        where T : Exception, new()
    {
        if (!condition)
        {
            throw new T();
        }
    }

    public static void IsTrue(bool condition, Func<Exception> exceptionFactory)
    {
        if (!condition)
        {
            throw exceptionFactory();
        }
    }

    public static void IsFalse(bool condition, string message) => IsTrue(!condition, message);

    public static void IsFalse<T>(bool condition)
        where T : Exception, new()
    {
        if (condition)
        {
            throw new T();
        }
    }

    public static void IsFalse(bool condition, Func<Exception> exceptionFactory)
    {
        if (condition)
        {
            throw exceptionFactory();
        }
    }

    public static void When(bool flag, Action action)
    {
        if (flag)
        {
            action();
        }
    }
}
