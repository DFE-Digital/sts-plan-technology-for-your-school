using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfe.PlanTech.Web.Helpers;

public static class ComponentViewsFactory
{
    public static string GetViewForType(object model) => $"Components/{model.GetType().Name}";
}
