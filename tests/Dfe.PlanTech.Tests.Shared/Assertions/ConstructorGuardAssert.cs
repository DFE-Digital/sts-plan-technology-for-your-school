using System.Reflection;
using Xunit;

namespace Dfe.PlanTech.Tests.Shared.Assertions;

public static class ConstructorGuardAssert
{
    public static void AllNonNullableParametersAreGuarded<T>(params object?[] validArguments)
    {
        var constructor = typeof(T)
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .Single(candidate => ArgumentsMatch(candidate.GetParameters(), validArguments));

        var nullabilityContext = new NullabilityInfoContext();

        foreach (var parameter in constructor.GetParameters())
        {
            if (parameter.ParameterType.IsValueType)
            {
                continue;
            }

            if (nullabilityContext.Create(parameter).WriteState == NullabilityState.Nullable)
            {
                continue;
            }

            var arguments = (object?[])validArguments.Clone();
            arguments[parameter.Position] = null;

            TargetInvocationException? invocationException = null;
            try
            {
                constructor.Invoke(arguments);
            }
            catch (TargetInvocationException exception)
            {
                invocationException = exception;
            }

            var innerException = invocationException?.InnerException;

            if (innerException is not ArgumentNullException argumentNullException)
            {
                Assert.Fail($"{typeof(T).Name}: parameter '{parameter.Name}' is not null-guarded.");
                return;
            }

            Assert.Equal(parameter.Name, argumentNullException.ParamName);
        }
    }

    private static bool ArgumentsMatch(ParameterInfo[] parameters, object?[] arguments) =>
        parameters.Length == arguments.Length
        && parameters
            .Zip(arguments)
            .All(pair =>
                pair.Second is null
                    ? !pair.First.ParameterType.IsValueType
                    : pair.First.ParameterType.IsInstanceOfType(pair.Second)
            );
}
