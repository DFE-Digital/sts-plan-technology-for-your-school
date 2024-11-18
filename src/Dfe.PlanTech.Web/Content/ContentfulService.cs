using Contentful.Core;
using Contentful.Core.Search;
using Dfe.PlanTech.Web.Models.Content;

namespace Dfe.PlanTech.Web.Content;

public class ContentfulService(
    [FromKeyedServices(ProgramExtensions.ContentAndSupportServiceKey)]
    IContentfulClient client)
    : IContentfulService
{
    private const int DefaultRequestDepth = 10;

    public async Task<IEnumerable<ContentSupportPage>> GetContentSupportPages(string field,
        string value, CancellationToken cancellationToken = default)
    {
        var builder = QueryBuilder<ContentSupportPage>.New.ContentTypeIs(nameof(ContentSupportPage))
            .FieldEquals($"fields.{field}", value)
            .Include(DefaultRequestDepth);

        var entries = await client.GetEntries(builder, cancellationToken);

        return entries ?? Enumerable.Empty<ContentSupportPage>();
    }
}
