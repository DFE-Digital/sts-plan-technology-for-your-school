using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped;
using Dfe.PlanTech.Domain.Content.Queries;

namespace Dfe.PlanTech.Web.Content;
public class ContentService : IContentService // This indicates that ContentService implements IContentService
{
    private readonly IGetContentSupportPageQuery _getContentSupportPageQuery;
    private readonly IModelMapper _modelMapper;

    public ContentService(
        IModelMapper modelMapper,
        IGetContentSupportPageQuery getContentSupportPageQuery)
    {
        _getContentSupportPageQuery = getContentSupportPageQuery;
        _modelMapper = modelMapper;
    }

    public async Task<CsPage?> GetContent(string slug, CancellationToken cancellationToken = default)
    {
        var resp = await _getContentSupportPageQuery.GetContentSupportPage(slug, cancellationToken);
        return resp == null ? null : _modelMapper.MapToCsPage(resp);
    }
}
