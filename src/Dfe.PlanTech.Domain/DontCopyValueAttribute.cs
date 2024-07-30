namespace Dfe.PlanTech.Domain;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
public class DontCopyValueAttribute : Attribute
{

}
