using System.Reflection;

namespace Dfe.PlanTech.Web.Helpers;

public class ComponentViewsFactory(
    ILogger<ComponentViewsFactory> logger
)
{
    private const string GENERATED_VIEW_NAMESPACE = "AspNetCoreGeneratedDocument";
    private const string SHARED_PATH = "Views_Shared";

    private readonly Type[] _viewTypes = GetSharedViewTypes().ToArray();

    /// <summary>
    /// Tries to find matching shared view for the passed model, based on the model's name
    /// </summary>
    /// <param name="model">The model for a view</param>
    /// <param name="viewPath">Found view path (if any matching view)</param>
    /// <returns>Whether a view was successfully found or not</returns>
    public bool TryGetViewForType(object model, out string? viewPath)
    {
        var componentTypeName = model.GetType().Name[0..^5].Replace("Component", "");
        var matchingViewType = _viewTypes.FirstOrDefault(FileNameMatchesComponentTypeName(componentTypeName));

        if (matchingViewType is null)
        {
            logger.LogWarning("Could not find matching view for {model}", model);
            viewPath = null;
            return false;
        }

        viewPath = GetFolderPathForType(matchingViewType);

        return true;
    }

    /// <summary>
    /// Gets file path to view from the Type name
    /// </summary>
    /// <remarks>
    /// Removes "Views_Shared_ from the name, then replaces all underscores with forward slashes
    /// </remarks>
    /// <param name="matchingViewType"></param>
    /// <returns>Folder path to the view for this type</returns>
    private static string GetFolderPathForType(Type matchingViewType) =>
        matchingViewType.Name!.Replace("Views_Shared_", "").Replace("_", "/");

    /// <summary>
    /// Does the passed component type name match the type name?
    /// </summary>
    /// <param name="componentTypeName"></param>
    /// <returns></returns>
    private static Func<Type, bool> FileNameMatchesComponentTypeName(string componentTypeName) =>
        type => type.Name.EndsWith($"_{componentTypeName}");

    /// <summary>
    /// Get all Types generated from Views that are in the "Shared" folder (or sub-folder)
    /// </summary>
    private static IEnumerable<Type> GetSharedViewTypes() =>
        Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(IsSharedViewType);

    /// <summary>
    /// Is this type a View type, which is in the Shared folder path?
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static bool IsSharedViewType(Type type) =>
        type.Namespace == GENERATED_VIEW_NAMESPACE && type.Name.StartsWith(SHARED_PATH);
}
