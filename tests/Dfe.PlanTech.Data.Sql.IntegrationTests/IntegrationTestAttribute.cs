using Xunit.Sdk;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class IntegrationTestAttribute : Attribute, ITraitAttribute
{
    public string Name => "Category";
    public string Value => "Integration";
}
