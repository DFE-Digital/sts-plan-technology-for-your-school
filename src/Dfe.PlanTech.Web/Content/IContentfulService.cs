using Dfe.PlanTech.Web.Models.Content;

namespace Dfe.PlanTech.Web.Content;

public interface IContentfulService
{
    Task<IEnumerable<ContentSupportPage>> GetContentSupportPages(string field, string value,
        CancellationToken cancellationToken = default);
}
