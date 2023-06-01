using System.Reflection;
using Microsoft.Extensions.Hosting.Internal;

namespace Dfe.PlanTech.Web.Helpers;

public class ComponentViewsFactory
{
    private readonly ComponentViewsFactoryOptions _options;

    public ComponentViewsFactory(ComponentViewsFactoryOptions options)
    {
        _options = options;
    }

    public bool TryGetViewForType(object model, out string viewPath)
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();

        viewPath = "";

        return true;
    }
}

public class ComponentViewsFactoryOptions
{
    public string[] ComponentViewPaths { get; init; } = Array.Empty<string>();
}