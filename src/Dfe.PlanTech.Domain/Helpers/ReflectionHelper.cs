using System.Reflection;

namespace Dfe.PlanTech.Domain.Helpers;

public static class ReflectionHelper
{
    public static IEnumerable<Type> GetTypesInheritingFrom<TBase>() => GetTypesInheritingFrom(typeof(TBase));

    public static IEnumerable<Type> GetTypesInheritingFrom(Type baseType, bool internalProjectsOnly = true) => AppDomain.CurrentDomain.GetAssemblies()
        .Where(assembly => IsNotTestProject(assembly, internalProjectsOnly))
        .SelectMany(AssemblyTypes)
        .Where(InheritsBaseType(baseType));

    private static Func<Type, bool> InheritsBaseType(Type baseType) => (type) => baseType.IsAssignableFrom(type) && type != baseType;

    private static Type[] AssemblyTypes(Assembly assembly) => assembly.GetTypes();

    private static bool IsNotTestProject(Assembly assembly, bool internalProjectsOnly)
    => assembly.FullName != null && !assembly.FullName.Contains("Test") && (!internalProjectsOnly || assembly.FullName.Contains("Dfe.PlanTech."));

    public static bool HasParameterlessConstructor(this Type type) => type.GetConstructor(Type.EmptyTypes) != null || type.GetConstructors().Length == 0;

    public static bool IsConcreteClass(this Type type) => !type.IsAbstract && !type.IsInterface;
}
