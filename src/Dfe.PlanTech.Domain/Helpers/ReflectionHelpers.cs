namespace Dfe.PlanTech.Domain.Helpers;

public static class ReflectionHelpers
{
    public static IEnumerable<Type> GetTypesInheritingFrom<TBase>()
    {
        var baseType = typeof(TBase);

        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany((assembley) => assembley.GetTypes())
            .Where((type) => baseType.IsAssignableFrom(type) && type != baseType);
    }
}
