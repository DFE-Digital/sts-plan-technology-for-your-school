namespace Dfe.PlanTech.Web.Extensions;

public static class WebHostEnvironmentExtensions
{
    public static bool IsTest(this IWebHostEnvironment env)
    {
        return env.IsEnvironment("Test");
    }

    public static bool IsNonProduction(this IWebHostEnvironment env)
    {
        return env.IsDevelopment()
               || env.IsStaging()
               || env.IsTest();
    }
}
