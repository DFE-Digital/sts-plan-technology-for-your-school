using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Helpers;

namespace Dfe.PlanTech.Web.ViewModels;

public class ActionViewModel
{
    [SetsRequiredMembers]
    public ActionViewModel(
        string actionName,
        string controllerName,
        string linkText,
        Dictionary<string, string>? routeValues = null
    )
    {
        if (
            string.IsNullOrWhiteSpace(controllerName)
            || string.IsNullOrWhiteSpace(actionName)
            || string.IsNullOrWhiteSpace(linkText)
        )
        {
            ActionName = string.Empty;
            ControllerName = string.Empty;
            LinkText = string.Empty;

            throw new InvalidDataException(
                $"{nameof(ActionViewModel)} must be provided with a controller name, action name, and link text"
            );
        }

        if (controllerName.EndsWith("Controller"))
        {
            controllerName = controllerName.GetControllerNameSlug();
        }

        ActionName = actionName;
        ControllerName = controllerName;
        LinkText = linkText;
        RouteValues = routeValues;
    }

    public required string ActionName { get; set; }
    public required string ControllerName { get; set; }
    public required string LinkText { get; set; }
    public Dictionary<string, string>? RouteValues { get; set; } = [];
}
