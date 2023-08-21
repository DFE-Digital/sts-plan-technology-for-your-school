using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

public class BaseController<TConcreteController> : Controller
{
    protected readonly ILogger<TConcreteController> logger;

    public BaseController(ILogger<TConcreteController> logger)
    {
        this.logger = logger;
    }

    protected static string SerialiseParameter(object? parameterToSerialise)
    {
        if (parameterToSerialise == null) throw new ArgumentNullException(nameof(parameterToSerialise));

        var parameterSerialised = Newtonsoft.Json.JsonConvert.SerializeObject(parameterToSerialise);

        return parameterSerialised ?? throw new NullReferenceException(nameof(parameterSerialised));
    }

    protected static T DeserialiseParameter<T>(object? parameterToDeserialise)
    {
        if (parameterToDeserialise == null) throw new ArgumentNullException(nameof(parameterToDeserialise));

        var parameterDeserialised = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(parameterToDeserialise as string ?? throw new ArithmeticException(nameof(parameterToDeserialise)));

        return parameterDeserialised ?? throw new NullReferenceException(nameof(parameterDeserialised));
    }
}