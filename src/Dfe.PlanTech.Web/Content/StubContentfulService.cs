using Contentful.Core;
using Contentful.Core.Configuration;
using Contentful.Core.Models;
using Dfe.PlanTech.Web.Models.Content;
using Newtonsoft.Json;
using File = System.IO.File;

namespace Dfe.PlanTech.Web.Content;

public class StubContentfulService(
    HttpClient httpClient,
    [FromKeyedServices(ProgramExtensions.ContentAndSupportServiceKey)]
    ContentfulOptions options)
    : ContentfulClient(httpClient, options), IContentfulService
{
    public async Task<IEnumerable<ContentSupportPage>> GetContentSupportPages(string field,
        string value, CancellationToken cancellationToken = default)
    {
        var json = await File.ReadAllTextAsync("StubData/ContentfulCollection.json",
            cancellationToken);
        var resp = JsonConvert.DeserializeObject<ContentSupportPage>(json);
        return resp == null
            ? new ContentfulCollection<ContentSupportPage>()
            : new ContentfulCollection<ContentSupportPage> { Items = [resp] };
    }
}
