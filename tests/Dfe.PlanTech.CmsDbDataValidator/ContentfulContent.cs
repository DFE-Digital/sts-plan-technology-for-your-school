using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;

namespace Dfe.PlanTech.CmsDbDataValidator;

public class ContentfulContent(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;
    private JsonArray? _entries;
    private readonly ContentfulExporter _contentfulExporter = new ContentfulExporter(configuration);
    public JsonArray Entries => _entries ?? throw new InvalidOperationException("Entries have not been loaded yet");

    public async Task Initialise()
    {
        _entries = await _contentfulExporter.GetAllEntriesAsJson();

        await SaveExportedEntries();
    }

    private async Task SaveExportedEntries()
    {
        var saveExport = bool.TryParse(_configuration["Contentful:SaveExport"], out bool shouldSaveExport) && shouldSaveExport;

        if (!saveExport)
            return;

        var contentfulEnvironment = _configuration["Contentful:Environment"];
        await File.WriteAllTextAsync($"contentful-export-{contentfulEnvironment}.json", JsonSerializer.Serialize(_entries));
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
            cleanedJsonNode.Add(field.Key, field.Value?.DeepClone());
        }

        return cleanedJsonNode;
    }
}
