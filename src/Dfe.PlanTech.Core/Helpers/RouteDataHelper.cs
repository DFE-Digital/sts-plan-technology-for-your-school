namespace Dfe.PlanTech.Core.Helpers;

public static class RouteDataHelper
{
    private const string Controller = "Controller";

    public static string GetControllerNameSlug(this string controllerName)
    {
        return controllerName.EndsWith(Controller)
            ? controllerName.Substring(0, controllerName.Length - Controller.Length)
            : controllerName;
    }
}
