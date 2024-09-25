using System.Reflection;

namespace Dfe.PlanTech;

public static class ReflectionHelpers
{
    public static async Task InvokeNonPublicAsyncMethod(this object toInvoke, string methodName, object?[]? parameters)
    {
        try
        {
            var task = (Task)toInvoke.InvokePublicMethod(methodName, parameters);
            await task;
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException ?? ex;
        }
    }

    public static object InvokePublicMethod(this object toInvoke, string methodName, object?[]? parameters)
    {
        try
        {
            var method = toInvoke.GetType().GetMethod(methodName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

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
