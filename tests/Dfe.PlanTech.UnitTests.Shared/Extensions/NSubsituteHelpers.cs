using System.Linq.Expressions;
using System.Reflection;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.UnitTests.Shared.Extensions;

public static class NSubstituteExtensions
{
    public static void HadMethodCalled<TSubstitute>(this TSubstitute substitute, string methodName, IEnumerable<object>? expectedArguments = null, int timesCalled = 1)
      where TSubstitute : class
    {
        var Calls = substitute.ReceivedCalls();

        var methodCalls = Calls.Where(call => call.GetMethodInfo().Name == methodName).ToArray();

        Assert.Equal(timesCalled, methodCalls.Length);

        if (expectedArguments is null)
            return;

        foreach (var methodCall in methodCalls)
        {
            var arguments = methodCall.GetArguments();
            var missingArguments = expectedArguments.Where(expectedArg => !arguments.Any(arg => expectedArg.Equals(arg))).ToArray();

            Assert.Empty(missingArguments);
        }
    }

    /// <summary>
    /// 
    /// Lifted from https://stackoverflow.com/questions/36276105/selecting-a-class-method-name-using-lambda-expression
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    private static string GetMethodCallName(LambdaExpression expression)
    {
        var unary = (UnaryExpression)expression.Body;
        var methodCall = (MethodCallExpression)unary.Operand;
        var constant = methodCall.Object as ConstantExpression ?? throw new Exception("Object is null");
        var memberInfo = constant!.Value as MemberInfo ?? throw new Exception("Method does not exist");
        return memberInfo.Name;
    }

}
