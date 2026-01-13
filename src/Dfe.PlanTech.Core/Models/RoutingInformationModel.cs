namespace Dfe.PlanTech.Core.Models;

public class RoutingInformationModel
{
    public string ActionName { get; set; }

    public string ControllerName { get; set; }

    public object? RoutingValues { get; set; }

    public RoutingInformationModel(string actionName, string controllerName, object? routingValues)
    {
        ActionName = actionName;
        ControllerName = controllerName;
        RoutingValues = routingValues;
    }
}
