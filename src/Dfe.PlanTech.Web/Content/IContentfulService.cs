using Dfe.PlanTech.Domain.Content.Models.ContentSupport;

namespace Dfe.PlanTech.Web.Content;

public interface IContentfulService
{
    Task<IEnumerable<ContentSupportPage>> GetContentSupportPages(string field, string value,
        CancellationToken cancellationToken = default);
}
