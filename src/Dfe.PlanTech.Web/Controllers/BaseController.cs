using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

public class BaseController<TConcreteController> : Controller
{
    protected readonly ILogger<TConcreteController> logger;

    public BaseController(ILogger<TConcreteController> logger)
    {
        this.logger = logger;
    }

    protected static T DeserialiseParameter<T>(object? parameterObject)
    {
        if (parameterObject == null) throw new ArgumentNullException(nameof(parameterObject));

        var parameterDeserialised = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(parameterObject as string ?? throw new ArithmeticException(nameof(parameterObject)));

        return parameterDeserialised ?? throw new NullReferenceException(nameof(parameterDeserialised));
    }
}