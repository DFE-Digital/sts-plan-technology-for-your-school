using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Helpers;

namespace Dfe.PlanTech.Web.ViewModels;

public class ActionViewModel
{
    public ActionViewModel() { } // For deserialization

    [SetsRequiredMembers]
    public ActionViewModel(
        string actionName,
        string controllerName,
        string linkText,
        object? routeValues = null
    )
    {
        if (
            string.IsNullOrWhiteSpace(actionName)
            || string.IsNullOrWhiteSpace(controllerName)
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

        if (routeValues is Dictionary<string, object> dictionary)
        {
            RouteValues = dictionary!;
        }
        else if (routeValues is RouteValueDictionary routeValueDictionary)
        {
            RouteValues = ConvertToDictionary(routeValueDictionary);
        }
        else if (routeValues is not null && !IsSimpleType(routeValues.GetType()))
        {
            try
            {
                RouteValues = ConvertToDictionary(new RouteValueDictionary(routeValues));
            }
            catch
            {
                RouteValues = null;
            }
        }
    }

    public required string ActionName { get; set; }
    public required string ControllerName { get; set; }
    public required string LinkText { get; set; }
    public Dictionary<string, object>? RouteValues { get; set; } = null;

    private static Dictionary<string, object> ConvertToDictionary(
        RouteValueDictionary routeValues
    ) => routeValues.ToDictionary(k => k.Key, k => k.Value ?? "");

    private static bool IsSimpleType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(Guid)
            || type == typeof(TimeSpan);
    }
}
