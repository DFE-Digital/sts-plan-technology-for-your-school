using System.Reflection;

namespace Dfe.PlanTech.UnitTests.Shared.Extensions;

public static class ReflectionHelpers
{
    public static async Task InvokeNonPublicAsyncMethod(this object toInvoke, string methodName, object[] parameters)
    {
        try
        {
            var task = (Task)toInvoke.InvokeNonPublicMethod(methodName, parameters);
            await task;
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException ?? ex;
        }
    }

    public static object InvokeNonPublicMethod(this object toInvoke, string methodName, object[] parameters)
        => toInvoke.InvokeNonPublicMethod(toInvoke.GetType(), methodName, parameters);

    public static object InvokeNonPublicMethod<T>(this object toInvoke, string methodName, object[] parameters)
        => toInvoke.InvokeNonPublicMethod(typeof(T), methodName, parameters);


    public static object InvokeNonPublicMethod(this object toInvoke, Type type, string methodName, object[] parameters)
    {
        try
        {
            var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (method == null)
            {
                throw new Exception("Couldn't find method " + methodName);
            }

            return method.Invoke(toInvoke, parameters)!;
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException ?? ex;
        }
    }


}
