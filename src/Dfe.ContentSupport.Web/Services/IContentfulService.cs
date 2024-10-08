using Dfe.ContentSupport.Web.ViewModels;

namespace Dfe.ContentSupport.Web.Services;

public interface IContentfulService
{
    Task<IEnumerable<ContentSupportPage>> GetContentSupportPages(string field, string value,
        CancellationToken cancellationToken = default);
}