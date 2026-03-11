namespace Dfe.PlanTech.Core.Helpers;

public static class RouteDataHelper
{
    public static string GetControllerNameSlug(this string controllerName)
    {
        return controllerName.Replace("Controller", "");
    }
}
