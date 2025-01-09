using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
using Dfe.PlanTech.Domain.Content.Queries;

namespace Dfe.PlanTech.Web.Content;
public class ContentService : IContentService // This indicates that ContentService implements IContentService
{
    private readonly IGetContentSupportPageQuery _getContentSupportPageQuery;

    public ContentService(
        IGetContentSupportPageQuery getContentSupportPageQuery)
    {
        _getContentSupportPageQuery = getContentSupportPageQuery;
    }

    public async Task<ContentSupportPage?> GetContent(string slug, CancellationToken cancellationToken = default)
    {
        var resp = await _getContentSupportPageQuery.GetContentSupportPage(slug, cancellationToken);
        return resp;
    }
}
