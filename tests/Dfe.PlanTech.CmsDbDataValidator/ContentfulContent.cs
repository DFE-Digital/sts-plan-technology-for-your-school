using System.Text.Json;
using System.Text.Json.Nodes;

namespace Dfe.PlanTech.CmsDbDataValidator;

public class ContentfulContent
{
  public readonly JsonNode Json;

  public ContentfulContent(string filePath)
  {
    Json = ParseJson(filePath);
  }

  public JsonNode ParseJson(string filePath)
  {
    var fileContent = File.ReadAllText(filePath);
    var json = JsonNode.Parse(fileContent);
    return json ?? throw new Exception("Null json");
  }

  public IEnumerable<JsonNode> GetEntriesForContentType(string contentType)
  {
    return Entries.Where(entry =>
    {
      var contentTypeIdNode = entry?["sys"]?["contentType"]?["sys"]?["id"];
      if (contentTypeIdNode == null)
      {
        Console.WriteLine("ContentType not found for entry");
        return false;
      }

      var contentTypeValue = contentTypeIdNode.GetValue<string>()!;

      return contentTypeValue == contentType;
    })!.Select(NormaliseNode!);
  }

  private static JsonObject NormaliseNode(JsonNode entry)
  {
    JsonObject normalised = NormaliseEntry(entry);

    var id = entry["sys"]?["id"]?.GetValue<string>() ?? throw new JsonException($"Couldn't find id for {entry}");
    normalised.Add("id", id);

    return normalised;
  }

  private static JsonObject NormaliseEntry(JsonNode entry)
  {
    var cleanedJsonNode = new JsonObject();
    var fields = (entry!["fields"] ?? throw new JsonException($"No fields for {entry}")).AsObject();

    foreach (var field in fields.AsObject())
    {
      var withoutLocalisation = field.Value!.WithoutLocalisation();
      cleanedJsonNode.Add(field.Key, withoutLocalisation);
    }

    return cleanedJsonNode;
  }

  private JsonArray Entries => Json["entries"]?.AsArray() ?? throw new JsonException("Couldn't find entries");
}