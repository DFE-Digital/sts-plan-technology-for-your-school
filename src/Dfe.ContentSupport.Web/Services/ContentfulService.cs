using Contentful.Core;
using Contentful.Core.Search;
using Dfe.ContentSupport.Web.Extensions;
using Dfe.ContentSupport.Web.ViewModels;

namespace Dfe.ContentSupport.Web.Services;

public class ContentfulService(
    [FromKeyedServices(WebApplicationBuilderExtensions.ContentAndSupportServiceKey)]
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