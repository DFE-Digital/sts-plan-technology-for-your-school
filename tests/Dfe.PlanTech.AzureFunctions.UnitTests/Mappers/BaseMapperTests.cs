using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dfe.PlanTech.AzureFunctions;

public abstract class BaseMapperTests
{
  protected readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
  };
}