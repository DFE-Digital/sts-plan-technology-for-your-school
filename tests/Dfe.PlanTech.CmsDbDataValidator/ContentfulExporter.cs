using System.Text.Json;
using System.Text.Json.Nodes;
using Contentful.Core;
using Contentful.Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace Dfe.PlanTech.CmsDbDataValidator;

public class ContentfulExporter
{
    private HttpClient _httpClient;

    public readonly ContentfulClient ContentfulClient;

    public ContentfulExporter(IConfiguration configuration)
    {
        _httpClient = new HttpClient();

        var contentfulOptions = new ContentfulOptions()
        {
            DeliveryApiKey = configuration["Contentful:DeliveryApiKey"],
            Environment = configuration["Contentful:Environment"],
            PreviewApiKey = configuration["Contentful:PreviewApiKey"],
            SpaceId = configuration["Contentful:SpaceId"],
            UsePreviewApi = bool.TryParse(configuration["Contentful:UsePreview"], out bool usePreviewApiKey) && usePreviewApiKey,
        };

        ContentfulClient = new ContentfulClient(_httpClient, contentfulOptions);
    }

    public async Task<JsonArray> GetAllEntriesAsJson()
    {
        var contentfulEntries = new JsonArray();
        int skip = 0;

        while (true)
        {
            var entriesChunk = await ContentfulClient.GetEntriesRaw(queryString: $"?skip={skip}");

            var serialised = JsonSerializer.Deserialize<JsonNode>(entriesChunk) ?? throw new JsonException("Could not serialise entries");
            var itemCount = serialised["total"]?.GetValue<int>() ?? 0;

            if (itemCount == 0)
                break;

            var items = serialised["items"]?.AsArray() ?? throw new JsonException("Could not serialise items");

            foreach (var item in items)
            {
                if (item == null)
                    continue;

                contentfulEntries.Add(item.DeepClone());
            }

            skip += 100;
        }

        return contentfulEntries;
    }
}
